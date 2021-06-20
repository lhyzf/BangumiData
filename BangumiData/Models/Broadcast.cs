using System;

namespace BangumiData.Models
{
    public struct Broadcast
    {
        public Broadcast(DateTimeOffset begin, int interval, IntervalUnit intervalUnit)
        {
            Begin = begin;
            Interval = interval;
            Unit = intervalUnit;
        }

        public static Broadcast Empty = new(DateTimeOffset.MinValue, 0, IntervalUnit.Day);
        public DateTimeOffset Begin { get; }
        public int Interval { get; }
        public IntervalUnit Unit { get; }

        /// <param name="input">
        /// Examples: <br/>
        /// 一次性："R/2020-01-01T13:00:00Z/P0D", <br/>
        /// 周播："R/2020-01-01T13:00:00Z/P7D", <br/>
        /// 日播："R/2020-01-01T13:00:00Z/P1D", <br/>
        /// 月播："R/2020-01-01T13:00:00Z/P1M"
        /// </param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string? input, out Broadcast result)
        {
            result = Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var ins = input.Split('/');
            if (ins.Length != 3)
            {
                return false;
            }
            if (ins[2].Length < 3)
            {
                return false;
            }
            // 周期数
            if (!int.TryParse(ins[2][1..^1], out int interval))
            {
                return false;
            }
            // 周期单位
            var intervalUnit = (IntervalUnit)ins[2][^1];
            if (!Enum.IsDefined(typeof(IntervalUnit), intervalUnit))
            {
                return false;
            }
            // 开始日期时间
            if (!DateTimeOffset.TryParse(ins[1], out var begin))
            {
                return false;
            }


            result = new Broadcast(begin, interval, intervalUnit);
            return true;
        }

        /// <summary>
        /// 按周期获取下一个时间
        /// </summary>
        /// <param name="intervalCount"></param>
        /// <returns></returns>
        public DateTimeOffset Next(int intervalCount)
        {
            return Unit switch
            {
                IntervalUnit.Day => Begin.AddDays(intervalCount * Interval),
                IntervalUnit.Month => Begin.AddMonths(intervalCount * Interval),
                _ => throw new InvalidOperationException("No IntervalUnit specified.")
            };
        }

        public override string? ToString()
        {
            return $"R/{Begin:yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'}/P{Interval}{Unit}";
        }
    }

    public enum IntervalUnit
    {
        Day = 'D',
        Month = 'M'
    }
}
