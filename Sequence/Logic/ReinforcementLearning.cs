using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sequence.Logic
{
    public enum Move
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class ReinforcementLearning
    {
        #region Set Custom Value
        private const int INIT_SCORE = 10;
        private const int GOAL_SCORE = 40;
        private const int SAME_GOAL_SCORE = 100;

        private static int GetScoreNextRecord(int before)
        {
            return before / 2;
            //return before - 5;
        }
        #endregion

        private int _repeat;
        private int? _minDistance;
        private Point _mapSize;
        private Point _currentCoor;
        private Point _destination;
        private readonly Point[] _obstacles = [];
        private readonly List<(Point, Move)> _record = []; //record path taken
        private readonly List<(Point Coor, int Distance)> _expandGoal = [];
        private readonly Dictionary<Point, Dictionary<Move, int?>> _qValue = []; //all weight by coor by direction

        public ReinforcementLearning(Point mapSize, Point destination, Point[] obstacle)
        {
            _mapSize = mapSize;
            _obstacles = obstacle;
            _destination = destination;

            _expandGoal.Add((destination, 0));
        }

        public int GetRepeat() => _repeat;
        public int GetMoveCount() => _record.Count;
        public int? GetMinDistance() => _minDistance;
        public bool IsComplete() => _expandGoal.FindAll(x => x.Coor == _currentCoor).Count > 0;
        public Point[] GetGoals() => _expandGoal.Select(x => x.Coor).ToArray();
        public Dictionary<Point, Dictionary<Move, int?>> GetQValue() => _qValue;

        public (bool IsGoal, Point Coor) Next()
        {
            bool isGoal = IsComplete();
            if (isGoal)
            {
                return (true, _currentCoor);
            }

            var next = NextMove();
            _record.Add((_currentCoor, next));

            _currentCoor = MoveNext(_currentCoor, next);

            isGoal = IsComplete();
            if (isGoal)
            {
                var score = GOAL_SCORE; //weight score
                var (Coor, Distance) = _expandGoal.Find(x => x.Coor == _currentCoor);

                if (_minDistance == null) //first arrive
                {
                    _minDistance = _record.Count + Distance;
                }
                else
                {
                    if (_minDistance > _record.Count + Distance) //short record
                    {
                        _minDistance = _record.Count + Distance;
                        score = GetMaxQValue(); //set max value is score
                    }
                    else if (_minDistance == _record.Count + Distance) //same record
                    {
                        score = SAME_GOAL_SCORE;
                    }
                }

                _record.Reverse();
                var list = _record.Distinct();

                //closer record give more weight
                foreach (var recordData in list)
                {
                    _qValue.TryGetValue(recordData.Item1, out var dic);
                    if (dic == null)
                    {
                        if (score < INIT_SCORE)
                        {
                            break;
                        }
                        dic = [];
                        dic.Add(recordData.Item2, score);
                        score = GetScoreNextRecord(score);
                    }
                    else
                    {
                        dic.TryGetValue(recordData.Item2, out var value);
                        if (value == null)
                        {
                            if (score < INIT_SCORE)
                            {
                                break;
                            }
                            dic.Add(recordData.Item2, score);
                            score = GetScoreNextRecord(score);
                        }
                        else
                        {
                            if (score < INIT_SCORE)
                            {
                                break;
                            }
                            dic[recordData.Item2] += score;
                            score = GetScoreNextRecord(score);
                        }
                    }
                    _qValue.TryAdd(recordData.Item1, dic);
                }

                _record.Clear();
                _currentCoor = new();
                _repeat++;

                const int EXPAND_REPEAT = 100;

                if (_repeat > _expandGoal.Count * EXPAND_REPEAT) //add new goal
                {
                    var max = GetMaxQValue();
                    var coor = _qValue.First(x => x.Value.Values.Max() == max).Key;

                    _record.Clear();
                    _qValue.Clear();
                    _minDistance = null;

                    List<(Point, int)> temp = [];
                    foreach (var index in Enum.GetValues<Move>())
                    {
                        var v = _expandGoal.FindAll(x => x.Coor == MoveNext(coor, index));
                        if (v.Count > 0)
                        {
                            temp.Add(v[0]);
                        }
                    }

                    if (temp.Count > 0)
                    {
                        _expandGoal.Add((coor, temp.Min(x => x.Item2) + 1));
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }

            return (isGoal, _currentCoor);
        }


        private static Point MoveNext(Point point, Move next)
        {
            Point add = next switch
            {
                Move.Up => new(0, -1),
                Move.Down => new(0, 1),
                Move.Left => new(-1, 0),
                Move.Right => new(1, 0),
                _ => throw new NotImplementedException()
            };
            return Add(point, add);
        }

        private static Point Add(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

        private int GetMaxQValue()
        {
            List<int> maxList = [];
            foreach (var item in _qValue.Values)
            {
                maxList.Add(item.Values.Max() ?? 0);
            }

            if (maxList.Count == 0)
                return INIT_SCORE * 10;

            return maxList.Max();
        }

        private Move NextMove()
        {
            while (true) //search next direction
            {
                _qValue.TryGetValue(_currentCoor, out var dic); //get move weights from current coor

                Dictionary<Move, int> scoreDic = []; //next move score

                if (dic == null) //no data is same weight
                {
                    scoreDic.Add(Move.Up, INIT_SCORE);
                    scoreDic.Add(Move.Down, INIT_SCORE);
                    scoreDic.Add(Move.Left, INIT_SCORE);
                    scoreDic.Add(Move.Right, INIT_SCORE);
                }
                else
                {
                    int weightSum = 0, emptyCount = 0;
                    Dictionary<Move, int?> tempDic = []; //move weights datas

                    foreach (var key in Enum.GetValues<Move>()) //qValue weight
                    {
                        dic.TryGetValue(key, out var value); //get weight from move
                        if (value != null)
                        {
                            weightSum += value.Value;
                            emptyCount++;
                            tempDic.Add(key, value);
                        }
                        else
                        {
                            tempDic.Add(key, null);
                        }
                    }
                    foreach (var key in Enum.GetValues<Move>()) //set empty move weights
                    {
                        if (tempDic[key] == null)
                        {
                            tempDic[key] = INIT_SCORE;
                        }
                    }
                    foreach (var key in Enum.GetValues<Move>()) //set scoreDic
                    {
                        scoreDic.Add(key, tempDic[key]!.Value);
                    }
                }

                //get next move by weight
                Move? move = null;
                int weight = 0;
                var select = new Random().Next(scoreDic.Values.Sum());
                foreach (var key in scoreDic.Keys)
                {
                    weight += scoreDic[key];
                    if (select < weight)
                    {
                        move = key;
                        break;
                    }
                }

                if (move == null)
                {
                    throw new Exception("fail get next move");
                }

                //check map outside
                if (_currentCoor.X == 0 && move == Move.Left ||
                    _currentCoor.Y == 0 && move == Move.Up ||
                    _currentCoor.X == _mapSize.X - 1 && move == Move.Right ||
                    _currentCoor.Y == _mapSize.Y - 1 && move == Move.Down)
                {
                    continue;
                }

                //check obstacle
                if (_obstacles.Contains(MoveNext(_currentCoor, move.Value)))
                    continue;

                return move.Value;
            }
        }


    }
}
