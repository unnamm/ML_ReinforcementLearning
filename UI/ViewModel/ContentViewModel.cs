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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UI.Model;

namespace UI.ViewModel
{
    public partial class ContentViewModel : ObservableRecipient, IRecipient<SettingSaveMessage>, IRecipient<CellMouseRightClickMessage>
    {
        private const string STOP = "Stop";
        private const string START = "Start";
        private readonly SolidColorBrush OBSTACLE_COLOR = Brushes.Red;

        public Log LogInstance { get; }

        [ObservableProperty] private int _mapX;
        [ObservableProperty] private int _mapY;
        [ObservableProperty] private int _delay = 100;
        [ObservableProperty] private int _repeat;
        [ObservableProperty] private int _minDistance;
        [ObservableProperty] private int _currentMoveCount;
        [ObservableProperty] private string _startButton = string.Empty;

        private ReinforcementLearning _rl = new();
        private CancellationTokenSource _cts = new();

        public ObservableCollection<PathItem> Coll { get; } = [];

        public ContentViewModel(Log log)
        {
            IsActive = true;

            LogInstance = log;

            RefreshMap();
        }

        private void RefreshMap()
        {
            StartButton = "Start";
            Repeat = 0;
            MinDistance = 0;
            CurrentMoveCount = 0;

            SettingData config = new();
            config.Load();

            MapX = config.MapSizeX;
            MapY = config.MapSizeY;

            Coll.Clear();
            for (int i = 0; i < MapX * MapY; i++)
            {
                Coll.Add(new());
                Coll[i].Coor = new(i % MapX, i / MapX);

                if (Coll[i].Coor == new System.Drawing.Point(config.GoalX, config.GoalY))
                {
                    Coll[i].Position = Visibility.Visible;
                    Coll[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Adjust;
                }
            }
            Coll[0].Position = Visibility.Visible;
        }

        public void Receive(SettingSaveMessage message)
        {
            RefreshMap();
        }

        [RelayCommand]
        public void StartAndStop()
        {
            if (StartButton == START)
            {
                StartButton = STOP;
                _cts = new();
                SettingData config = new();
                config.Load();

                List<System.Drawing.Point> list = [];
                foreach (var item in Coll)
                {
                    if (item.Background == OBSTACLE_COLOR)
                    {
                        list.Add(item.Coor);
                    }
                }

                _rl.Init(new(MapX, MapY), new(config.GoalX, config.GoalY), list.ToArray());
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
            _rl = new();
            Cancel();
            RefreshMap();
        }

        private void Cancel()
        {
            _cts.Cancel();
            StartButton = START;
        }

        private async void Run()
        {
            try
            {
                while (true) //init
                {
                    _rl.SetStartPos();
                    CurrentMoveCount = 0;

                    while (true) //move next
                    {
                        await Task.Delay(Delay);
                        _cts.Token.ThrowIfCancellationRequested();

                        var (IsGoal, Direction, Coor) = _rl.Next();

                        foreach (var item in Coll)
                        {
                            if (item.Kind != MaterialDesignThemes.Wpf.PackIconKind.Adjust)
                            {
                                item.Position = Visibility.Hidden;
                            }
                        }

                        Coll[Coor.X + Coor.Y * MapX].Position = Visibility.Visible; //current coor

                        CurrentMoveCount++;

                        if (IsGoal)
                        {
                            Repeat++;
                            MinDistance = _rl.GetMinDistance()!.Value;
                            Coll[0].Position = Visibility.Visible;

                            var q = _rl.GetQValue();
                            foreach (var item in Coll)
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
            catch (Exception ex)
            {
                LogInstance.Write(ex.Message);
            }
        }

        public void Receive(CellMouseRightClickMessage message)
        {
            if (_rl.GetMoveCount() != 0)
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
        }
    }
}
