using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{

    public class ContractScheduleWeek : AggregateEntity, IContractScheduleWeek
    {
        private int _weekOrder;
        private readonly IDictionary<DayOfWeek, bool> workDays = new Dictionary<DayOfWeek, bool>(7);

        /// <summary>
        /// Sort order for Week
        /// </summary>
        public virtual int WeekOrder
        {
            get { return _weekOrder; }
            set { _weekOrder = value; }
        }

        /// <summary>
        /// Check if given day is a work day for the week
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <returns>True if it's a work day, otherwise False.</returns>
        public virtual bool IsWorkday(DayOfWeek dayOfWeek)
        {
            return this[dayOfWeek];
        }

        /// <summary>
        /// Get information about given week day
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <returns>True if it's a work day, otherwise False.</returns>
        public virtual bool this[DayOfWeek dayOfWeek]
        {
            get
            {
                bool value;
                if (workDays.TryGetValue(dayOfWeek,out value))
                    return value;

                return false;
            }
        }

        /// <summary>
        /// Add new definition to collection (or replace old)
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <param name="isWorkday">True if day is work day</param>
        public virtual void Add(DayOfWeek dayOfWeek, bool isWorkday)
        {
            if (workDays.ContainsKey(dayOfWeek))
                workDays[dayOfWeek] = isWorkday;
            else
                workDays.Add(dayOfWeek, isWorkday);
        }

        /// <summary>
        /// Removes an item from collection
        /// </summary>
        /// <param name="dayOfWeek"></param>
        public virtual void Remove(DayOfWeek dayOfWeek)
        {
            if (workDays.ContainsKey(dayOfWeek))
                workDays.Remove(dayOfWeek);
        }

        /// <summary>
        /// Get the number of explicit defined days
        /// </summary>
        public virtual int Count
        {
            get { return workDays.Count; }
        }
    }
}