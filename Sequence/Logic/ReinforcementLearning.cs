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
        private Point _current; //current coor
        private Point _mapSize;
        private Point _destination;
        private (int? min, int score) _scoreData = (null, 10); //weight score

        private readonly Point[] _obstacle = [];
        private readonly List<(Point, Move)> _record = []; //record path taken
        private readonly Dictionary<Point, Dictionary<Move, int?>> _qValue = []; //all weight by coor by direction

        public ReinforcementLearning(Point mapSize, Point destination, Point[] obstacle)
        {
            _mapSize = mapSize;
            _obstacle = obstacle;
            _destination = destination;
        }

        public int GetMoveCount() => _record.Count;

        public int? GetMinDistance() => _scoreData.min;

        public Dictionary<Point, Dictionary<Move, int?>> GetQValue() => _qValue;

        public (bool IsGoal, Point Coor) Next()
        {
            if (_current == _destination)
            {
                return (true, _destination);
            }

            var next = NextMove();
            _record.Add((_current, next));

            //set current coor
            (next switch
            {
                Move.Up => () => { _current.Y--; }
                ,
                Move.Down => () => { _current.Y++; }
                ,
                Move.Left => () => { _current.X--; }
                ,
                Move.Right => (Action)(() => { _current.X++; }),
                _ => throw new Exception()
            })();

            bool isGoal = _current == _destination;
            if (isGoal)
            {
                var score = 10; //weight score

                if (_scoreData.min == null) //first arrive
                {
                    _scoreData.min = _record.Count;
                }
                else
                {
                    if (_scoreData.min > _record.Count) //short record
                    {
                        _scoreData.score += (_scoreData.min - _record.Count).Value; //plus shortened record
                        _scoreData.min = _record.Count; //set min record

                        score = _scoreData.score;

                        List<int> maxList = [];
                        foreach (var item in _qValue.Values)
                        {
                            maxList.Add(item.Values.Max() ?? 0);
                        }
                        score += maxList.Max();
                    }
                    else if (_scoreData.min == _record.Count) //same record
                    {
                        _scoreData.score += 10; //same record is plus weight

                        score = _scoreData.score;
                    }
                }

                //distinct
                List<(Point, Move)> distinctRecord = [];
                foreach (var item in _record)
                {
                    if (distinctRecord.Any(x => x.Item1 == item.Item1 && x.Item2 == item.Item2) == false)
                    {
                        distinctRecord.Add(item);
                    }
                }

                foreach (var recordData in distinctRecord)
                {
                    _qValue.TryGetValue(recordData.Item1, out var dic);
                    if (dic == null)
                    {
                        dic = [];
                        dic.Add(recordData.Item2, score);
                    }
                    else
                    {
                        dic.TryGetValue(recordData.Item2, out var value);
                        if (value == null)
                        {
                            dic.Add(recordData.Item2, score);
                        }
                        else
                        {
                            dic[recordData.Item2] += score;
                        }
                    }
                    _qValue.TryAdd(recordData.Item1, dic);
                }

                _record.Clear();
                _current = new();
            }

            return (isGoal, _current);
        }

        private Move NextMove()
        {
            while (true)
            {
                _qValue.TryGetValue(_current, out var dic); //get move weights from current coor

                Dictionary<Move, int> scoreDic = [];

                if (dic == null) //no data is same weight
                {
                    scoreDic.Add(Move.Up, 1);
                    scoreDic.Add(Move.Down, 1);
                    scoreDic.Add(Move.Left, 1);
                    scoreDic.Add(Move.Right, 1);
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
                            tempDic[key] = weightSum / emptyCount;
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
                if (_current.X == 0 && move == Move.Left ||
                    _current.Y == 0 && move == Move.Up ||
                    _current.X == _mapSize.X - 1 && move == Move.Right ||
                    _current.Y == _mapSize.Y - 1 && move == Move.Down)
                {
                    continue;
                }

                //check obstacle
                Point p = move switch
                {
                    Move.Up => new Point(_current.X, _current.Y - 1),
                    Move.Down => new Point(_current.X, _current.Y + 1),
                    Move.Left => new Point(_current.X - 1, _current.Y),
                    Move.Right => new Point(_current.X + 1, _current.Y),
                    _ => throw new Exception()
                };
                if (_obstacle.Contains(p))
                    continue;

                return move.Value;
            }
        }



    }
}
