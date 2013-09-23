using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// TimePeriod struct
    /// </summary>
    [Serializable]
    public struct TimePeriod : IComparable
    {
        #region Fields
        private MinMax<TimeSpan> period;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        public TimeSpan EndTime
        {
            get { return period.Maximum; }
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public TimeSpan StartTime
        {
            get { return period.Minimum; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriod"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public TimePeriod(TimeSpan start, TimeSpan end)
        {
            period = new MinMax<TimeSpan>(start, end);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriod"/> struct.
        /// </summary>
        /// <param name="startAndEnd">The start and end.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-28
        /// </remarks>
        public TimePeriod(string startAndEnd)
        {
            TimePeriod tp;
            if (TryParse(startAndEnd, out tp))
            {
                period = new MinMax<TimeSpan>(tp.StartTime, tp.EndTime);
            } else
            {
                throw new ArgumentException("Provided Start and End time could not be parsed.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriod"/> struct.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="startMinutes">The start minutes.</param>
        /// <param name="endHour">The end hour.</param>
        /// <param name="endMinutes">The end minutes.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public TimePeriod(int startHour, int startMinutes, int endHour, int endMinutes) 
            : this(new TimeSpan(startHour, startMinutes, 0), new TimeSpan(endHour, endMinutes, 0))
        {
        }

        /// <summary>
        /// Tries the parse. (*LOL* Actually tries to parse a TimePeriod string.)
        /// </summary>
        /// <param name="value">The TimePeriod as string.</param>
        /// <param name="timePeriod">The time period.</param>
        /// <returns></returns>
        public static bool TryParse(string value, out TimePeriod timePeriod)
        {
            timePeriod = new TimePeriod();
            // Check if string contains a - separator.
            if (!value.Contains("-")) { return false; }

            string[] sae = value.Split(new[] {"-"}, StringSplitOptions.None);
            // Check if string contains two items.
            if (sae.Length != 2) { return false; }

            TimeSpan start;
            if (!TimeHelper.TryParse(sae[0], out start))
            {
                return false;
            }

            TimeSpan end;
            if (!TimeHelper.TryParse(sae[1], out end))
            {
                return false;
            }

            if (end == start &&
                end.TotalMinutes == 0d)
            {
                end = end.Add(TimeSpan.FromDays(1));
            }
            if (end < start)
            {
                end = end.Add(TimeSpan.FromDays(1));
            }

            timePeriod = new TimePeriod(start, end);
            return true;
        }

        #region IEquatable<TimePeriod> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(TimePeriod other)
        {
            return period == other.period;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TimePeriod))
            {
                return false;
            }
            return Equals((TimePeriod) obj);
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(TimePeriod per1, TimePeriod per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(TimePeriod per1, TimePeriod per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Implements the operator greater than.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-20
        /// </remarks>
        public static bool operator >(TimePeriod per1, TimePeriod per2)
        {
            return per1.StartTime > per2.StartTime;
        }


        /// <summary>
        /// Implements the operator less than.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-20
        /// </remarks>
        public static bool operator <(TimePeriod per1, TimePeriod per2)
        {
            return per1.StartTime < per2.StartTime;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return period.GetHashCode();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. 
        /// 
        /// The return value has these meanings: 
        /// 
        /// Value Meaning Less than zero This instance is less than <paramref name="obj"/>. 
        /// Zero This instance is equal to <paramref name="obj"/>. 
        /// Greater than zero This instance is greater than <paramref name="obj"/>.
        /// 
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="obj"/> is not the same type as this instance. </exception>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        public int CompareTo(object obj)
        {
            TimePeriod timePeriod = (TimePeriod) obj;
            if (timePeriod.StartTime > StartTime)
            {
                return -1;
            }
            if(timePeriod.StartTime < StartTime)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Returns string representation of TimePeriod.
        /// Example: 02:00:00-12:00:00
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            DateTime dtStart = DateTime.MinValue.Add(StartTime);
            DateTime dtEnd = DateTime.MinValue.Add(EndTime);
            String periodString = dtStart.ToLongTimeString() + "-" + dtEnd.ToLongTimeString();
            return periodString;
        }
        /// <summary>
        /// Returns string representation of TimePeriod.
        /// Example: 02:00 - 12:00 
        /// </summary>
        /// <returns></returns>
        public string ToShortTimeString()
        {
            return TimeHelper.TimeOfDayFromTimeSpan(StartTime) + " - " + TimeHelper.TimeOfDayFromTimeSpan(EndTime);
        }

        ///<summary>
        /// Returns string representation of TimePeriod in a specific culture.
        /// Example: 02:00 AM - 10:00 AM  
        ///</summary>
        ///<param name="culture"></param>
        ///<returns></returns>
        public string ToShortTimeString(IFormatProvider culture)
        {
            return TimeHelper.TimeOfDayFromTimeSpan(StartTime, culture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(EndTime, culture);
        }

        #endregion


        #region Contains


        /// <summary>
        /// Determines whether this period contains the specified time span.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>
        /// 	<c>true</c> if this period containsthe specified time span; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public bool Contains(TimeSpan timeSpan)
        {
            return (StartTime <= timeSpan && EndTime > timeSpan);
        }
        /// <summary>
        /// Determines whether [contains] [the specified period].
        /// </summary>
        /// <param name="containsPeriod">The period.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        public bool Contains(TimePeriod containsPeriod)
        {
            return (ContainsPart(containsPeriod.StartTime) && ContainsPart(containsPeriod.EndTime));
        }

        /// <summary>
        /// Determines whether the specified time span contains part.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>
        /// 	<c>true</c> if the specified time span contains part; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        public bool ContainsPart(TimeSpan timeSpan)
        {
            return (timeSpan <= EndTime && timeSpan >= StartTime);
        }

        #endregion

        /// <summary>
        /// Does the two periods intersect.
        /// </summary>
        /// <param name="intersectPeriod">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public bool Intersect(TimePeriod intersectPeriod)
        {
            if (Contains(intersectPeriod.StartTime)||Contains(intersectPeriod.EndTime.Subtract(new TimeSpan(1))))
                return true;
            if (intersectPeriod.Contains(this))
                return true;
            //if (Contains(period)) this is the same as the row before
            //    return true;
            return false;
        }

        /// <summary>
        /// Returns a TimePeriod representing the spanning time of this TimePeriod.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 9.12.2007
        /// </remarks>
        public TimeSpan SpanningTime()
        {
            return EndTime.Subtract(StartTime);
        }

        /// <summary>
        /// Returns an list of ordered by StartTime
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-04-23
        /// </remarks>
        public static IList<TimePeriod> Combine(IList<TimePeriod> periods)
        {
            List<TimePeriod> retList = new List<TimePeriod>();
            List<TimePeriod> orderedList = (List<TimePeriod>)periods;
            orderedList.Sort();
            foreach (TimePeriod period in orderedList)
            {
                if (retList.Count != 0)
                {
                    if (retList[retList.Count - 1].Intersect(period))
                    {

                        if (retList[retList.Count - 1].EndTime < period.EndTime) 
                        {
                            TimePeriod newTimePeriod = new TimePeriod(retList[retList.Count - 1].StartTime, period.EndTime);
                            retList.Remove(retList[retList.Count - 1]);
                            retList.Add(newTimePeriod);
                        }
                    }
                    else retList.Add(period);
                }
                else retList.Add(period);
            }

            return retList;
           



        }


        /// <summary>
        /// Aligns to minutes.
        /// </summary>
        /// <param name="timePeriod">The time period.</param>
        /// <param name="minutes">The minutes.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-21
        /// </remarks>
        public static TimePeriod AlignToMinutes(TimePeriod timePeriod, int minutes)
        {
            long minutesTicks = new TimeSpan(0, minutes, 0).Ticks;
            TimeSpan startTimeSpan = TimeSpan.FromTicks(divideAndMultiply(timePeriod.StartTime.Ticks, minutesTicks));
            TimeSpan endTimeSpan;
            if (timePeriod.EndTime.Minutes == 0)
            {
                endTimeSpan = timePeriod.EndTime;
            }
            else
            {
                endTimeSpan = TimeSpan.FromTicks(divideAndMultiply(timePeriod.EndTime.Ticks, minutesTicks) + minutesTicks);
            }
            TimePeriod newTimePeriod = new TimePeriod(startTimeSpan, endTimeSpan);
            return newTimePeriod;
        }

        private static long divideAndMultiply(long value, long divisor)
        {
            return (value / divisor) * divisor;
        }

    }
}