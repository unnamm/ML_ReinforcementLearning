using Common;
using Common.Config;
using Common.Message;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sequence.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using UI.Model;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace UI.ViewModel
{
    public partial class ContentViewModel : ObservableRecipient, IRecipient<SettingSaveMessage>, IRecipient<CellMouseRightClickMessage>
    {
        private const string STOP = "Stop";
        private const string START = "Start";

        private readonly SettingData _config = new(); //setting config
        private readonly SolidColorBrush OBSTACLE_COLOR = Brushes.Red; //obstacle backgroud color

        public Log LogInstance { get; }

        [ObservableProperty] private int _mapX; //map width size
        [ObservableProperty] private int _mapY; //map height size
        [ObservableProperty] private int _delay = 100; //current coordinate move delay
        [ObservableProperty] private int _repeat; //goal repeat count
        [ObservableProperty] private int _minDistance; //search min distance
        [ObservableProperty] private int _currentMoveCount; //current move count in one search
        [ObservableProperty] private string _startButton = string.Empty; //start <-> stop button name

        private ReinforcementLearning _learn;
        private CancellationTokenSource _cts = new(); //stop button token

        public ObservableCollection<PathItem> CellCollection { get; } = []; //cell width * height

        public ContentViewModel(Log log)
        {
            IsActive = true;
            LogInstance = log;

            InitButton();
        }

        private void RefreshMap()
        {
            _config.Load();

            Repeat = 0;
            MinDistance = 0;
            StartButton = START;
            CurrentMoveCount = 0;

            MapX = _config.MapSizeX;
            MapY = _config.MapSizeY;

            List<System.Drawing.Point> list = [];
            foreach (var item in CellCollection)
            {
                if (item.Background == OBSTACLE_COLOR)
                {
                    list.Add(item.Coor);
                }
            }

            _learn = new(new(MapX, MapY), new(_config.GoalX, _config.GoalY), list.ToArray());
        }

        public void Receive(SettingSaveMessage message)
        {
            InitButton();
        }

        [RelayCommand]
        public void StartAndStop()
        {
            if (StartButton == START)
            {
                StartButton = STOP;
                _cts = new();

                Run();
            }
            else
            {
                Cancel();
            }
        }

        [RelayCommand]
        public void InitButton()
        {
            Cancel();
            RefreshMap();

            CellCollection.Clear();
            for (int i = 0; i < MapX * MapY; i++)
            {
                CellCollection.Add(new());
                CellCollection[i].Coor = new(i % MapX, i / MapX);

                if (CellCollection[i].Coor == new System.Drawing.Point(_config.GoalX, _config.GoalY))
                {
                    CellCollection[i].Position = Visibility.Visible;
                    CellCollection[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Adjust;
                }
            }
            CellCollection[0].Position = Visibility.Visible;
            CellCollection[0].Kind = PathItem.PLAYER;
        }

        [RelayCommand]
        public void DataReset()
        {
            Cancel();
            RefreshMap();

            foreach (var item in CellCollection)
            {
                if (item.Coor == new System.Drawing.Point(_config.GoalX, _config.GoalY))
                {
                    item.Position = Visibility.Visible;
                }
                else
                {
                    item.Position = Visibility.Hidden;
                }
                item.UpArrow = Visibility.Hidden;
                item.DownArrow = Visibility.Hidden;
                item.LeftArrow = Visibility.Hidden;
                item.RightArrow = Visibility.Hidden;
            }
            CellCollection[0].Position = Visibility.Visible;
        }

        private void Cancel()
        {
            _cts.Cancel();
            StartButton = START;
        }

        /// <summary>
        /// repeat all
        /// </summary>
        private async void Run()
        {
            //try
            {
                while (true) //repeat search
                {
                    if (_learn.IsComplete())
                    {
                        LogInstance.Write("Complete");
                        Cancel();
                    }

                    while (true) //repeat next move
                    {
                        await Task.Delay(Delay);
                        try
                        {
                            _cts.Token.ThrowIfCancellationRequested();
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        var (IsGoal, Coor) = _learn.Next();

                        CurrentMoveCount = _learn.GetMoveCount();

                        foreach (var item in CellCollection)
                        {
                            item.Position = Visibility.Hidden;

                            if (_learn.GetGoals().Contains(item.Coor))
                            {
                                item.Kind = PathItem.GOAL;
                                item.Position = Visibility.Visible;
                            }

                            if (item.Coor == Coor)
                            {
                                item.Kind = PathItem.PLAYER;
                                item.Position = Visibility.Visible;
                            }
                        }

                        if (IsGoal)
                        {
                            Repeat = _learn.GetRepeat();
                            MinDistance = _learn.GetMinDistance() ?? 0;
                            CellCollection[0].Position = Visibility.Visible;

                            var q = _learn.GetQValue();
                            foreach (var item in CellCollection)
                            {
                                q.TryGetValue(item.Coor, out var pair);
                                if (pair == null)
                                {
                                    item.UpArrow = Visibility.Hidden;
                                    item.DownArrow = Visibility.Hidden;
                                    item.LeftArrow = Visibility.Hidden;
                                    item.RightArrow = Visibility.Hidden;
                                }
                                else
                                {
                                    pair.TryGetValue(Move.Up, out var weight);
                                    if (weight == null)
                                    {
                                        item.UpArrow = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        item.UpArrow = Visibility.Visible;
                                        item.UpWeight = weight.Value;
                                    }

                                    pair.TryGetValue(Move.Down, out weight);
                                    if (weight == null)
                                    {
                                        item.DownArrow = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        item.DownArrow = Visibility.Visible;
                                        item.DownWeight = weight.Value;
                                    }

                                    pair.TryGetValue(Move.Left, out weight);
                                    if (weight == null)
                                    {
                                        item.LeftArrow = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        item.LeftArrow = Visibility.Visible;
                                        item.LeftWeight = weight.Value;
                                    }

                                    pair.TryGetValue(Move.Right, out weight);
                                    if (weight == null)
                                    {
                                        item.RightArrow = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        item.RightArrow = Visibility.Visible;
                                        item.RightWeight = weight.Value;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    LogInstance.Write(ex.Message);
            //}
        }

        public void Receive(CellMouseRightClickMessage message)
        {
            if (_learn.GetMoveCount() != 0 || _learn.GetMinDistance() != null)
            {
                LogInstance.Write("obstacle can only be added in initialize state");
                return;
            }

            if (message.Item.Background == OBSTACLE_COLOR)
            {
                message.Item.Background = Brushes.Transparent;
            }
            else
            {
                message.Item.Background = OBSTACLE_COLOR;
            }

            List<System.Drawing.Point> list = [];
            foreach (var item in CellCollection)
            {
                if (item.Background == OBSTACLE_COLOR)
                {
                    list.Add(item.Coor);
                }
            }
            _learn = new(new(MapX, MapY), new(_config.GoalX, _config.GoalY), list.ToArray());
        }
    }
}
