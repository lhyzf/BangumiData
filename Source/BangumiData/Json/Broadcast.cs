using System;

namespace BangumiData.Json
{
    public class Broadcast
    {
        public const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        public Broadcast(DateTimeOffset begin, int interval, IntervalUnit intervalUnit)
        {
            Begin = begin;
            Interval = interval;
            Unit = intervalUnit;
        }

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
        public static bool TryParse(string? input, out Broadcast? result)
        {
            result = null;
            if (input == null)
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
#if NETSTANDARD2_0
            if (!int.TryParse(ins[2].Substring(1, ins[2].Length - 2), out int interval))
#else
            if (!int.TryParse(ins[2][1..^1], out int interval))
#endif
            {
                return false;
            }
            // 周期单位
#if NETSTANDARD2_0
            var intervalUnit = (IntervalUnit)ins[2][ins[2].Length - 1];
#else
            var intervalUnit = (IntervalUnit)ins[2][^1];
#endif
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

        public DateTimeOffset Round(DateTimeOffset dateTimeOffset)
        {
            return Unit switch
            {
                IntervalUnit.Day => Begin.AddDays(Interval * (int)Math.Round((dateTimeOffset - Begin.Date).Days / (double)Interval, MidpointRounding.AwayFromZero)),
                IntervalUnit.Month => Begin.AddYears(dateTimeOffset.Year - Begin.Year).AddMonths(dateTimeOffset.Month - Begin.Month),
                _ => throw new InvalidOperationException("No IntervalUnit specified.")
            };
        }

        public override string? ToString()
        {
            return $"R/{Begin.ToString(DateTimeFormat)}/P{Interval}{(char)Unit}";
        }

        public enum IntervalUnit
        {
            Day = 'D',
            Month = 'M'
        }
    }
}
