using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Common
{
    public class WeekDayDistribution<T>
    {
        public WeekDayDistribution(T sunday, T monday, T tuesday, T wednesday, T thursday, T friday, T saturday)
        {
            Sunday = sunday;
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
        }

        public T Sunday { get; set; }
        public T Monday { get; set; }
        public T Tuesday { get; set; }
        public T Wednesday { get; set; }
        public T Thursday { get; set; }
        public T Friday { get; set; }
        public T Saturday { get; set; }
        

        public override bool Equals(object? obj)
        {
            return obj is WeekDayDistribution<T> wd
                && (Sunday is not null && Sunday.Equals(wd.Sunday) || Sunday is null && wd.Sunday is null)
                && (Monday is not null && Monday.Equals(wd.Monday) || Monday is null && wd.Monday is null)
                && (Tuesday is not null && Tuesday.Equals(wd.Tuesday) || Tuesday is null && wd.Tuesday is null)
                && (Wednesday is not null && Wednesday.Equals(wd.Wednesday) || Wednesday is null && wd.Wednesday is null)
                && (Thursday is not null && Thursday.Equals(wd.Thursday) || Thursday is null && wd.Thursday is null)
                && (Friday is not null && Friday.Equals(wd.Friday) || Friday is null && wd.Friday is null)
                && (Saturday is not null && Saturday.Equals(wd.Saturday) || Saturday is null && wd.Saturday is null)
            ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int left = (Sunday is null ? 0 : Sunday.GetHashCode()) +
                    (Monday is null ? 0 : Monday.GetHashCode() * 31) +
                    (Tuesday is null ? 0 : Tuesday.GetHashCode() * 13) +
                    (Wednesday is null ? 0 : Wednesday.GetHashCode() * 7);

                int right = (Thursday is null ? 0 : Thursday.GetHashCode()) +
                    (Friday is null ? 0 : Friday.GetHashCode() * 17) +
                    (Saturday is null ? 0 : Saturday.GetHashCode() * 23);

                return left ^ right;
            }
        }
    }
}
