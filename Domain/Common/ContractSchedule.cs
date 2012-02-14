using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using InParameter=Teleopti.Interfaces.Domain.InParameter;

namespace Teleopti.Ccc.Domain.Common
{

    public class ContractSchedule : AggregateRootWithBusinessUnit, IContractSchedule, IDeleteTag
    {
        private Description _description;
        private readonly ISet<IContractScheduleWeek> _contractScheduleWeeks; //byt till BCL's ISet<T> när vi går över till .net 4.0!
        private bool _isDeleted;

        /// <summary>
        /// Creates a new instance of ContractSchedule
        /// </summary>
        /// <param name="name">Name of ContractSchedule</param>
        public ContractSchedule(string name)
        {
            _description = new Description(name);
            _contractScheduleWeeks = new HashedSet<IContractScheduleWeek>();
        }

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected ContractSchedule()
        {
        }

        /// <summary>
        /// Description of ContractSchedule
        /// </summary>
        public virtual Description Description
        {
            get { return _description; }
            set{_description = value;}
        }

        /// <summary>
        /// Gets the contained weeks in contract schedule
        /// </summary>
        public virtual IEnumerable<IContractScheduleWeek> ContractScheduleWeeks
        {
            get { return _contractScheduleWeeks; }
        }

        /// <summary>
        /// Adds the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        public virtual void AddContractScheduleWeek(IContractScheduleWeek contractScheduleWeek)
        {
            InParameter.NotNull("contractScheduleWeek", contractScheduleWeek);
            ((AggregateEntity)contractScheduleWeek).SetParent(this);

            //Sets the week order to the next index of the collection
            contractScheduleWeek.WeekOrder = _contractScheduleWeeks.Count;
            _contractScheduleWeeks.Add(contractScheduleWeek); 
        }

        /// <summary>
        /// Removes the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        public virtual void RemoveContractScheduleWeek(IContractScheduleWeek contractScheduleWeek)
        {
            InParameter.NotNull("contractScheduleWeek", contractScheduleWeek);
            ((AggregateEntity)contractScheduleWeek).SetParent(this);
            _contractScheduleWeeks.Remove(contractScheduleWeek);
            
            resetWeekOrdersUponDeletion();
        }

         /// <summary>
        /// Clears the contract schedule week collection
        /// </summary>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        public virtual void ClearContractScheduleWeeks()
        {
             _contractScheduleWeeks.Clear();
        }

        /// <summary>
        /// Gets the number of anticipated work days.
        /// </summary>
        /// <param name="firstDayOfWeek">The first day of week.</param>
        /// <param name="startDayOfWeek">The start day of week.</param>
        /// <param name="days">The days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        public virtual int GetWorkdays(DayOfWeek firstDayOfWeek, DayOfWeek startDayOfWeek, int days)
        {
            if (_contractScheduleWeeks.Count==0) return days;

            int workDays = 0;
            for (int dayIndex = 0; dayIndex < days; dayIndex++)
            {
                if (IsWorkday(firstDayOfWeek, startDayOfWeek, dayIndex))
                    workDays++;
            }

            return workDays;
        }


        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        public virtual bool IsChoosable
        {
            get { return !IsDeleted; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual bool IsWorkday(DayOfWeek firstDayOfWeek, DayOfWeek startDayOfWeek, int dayIndex)
        {
            if (_contractScheduleWeeks.Count == 0)
                return true;

            var weeks = _contractScheduleWeeks.OrderBy(w => w.WeekOrder).ToList();
            bool ret = true;
            int weekCount = weeks.Count;
            int currentWeek = 0;
            DayOfWeek currentDayOfWeek = startDayOfWeek;
            for (int currentDay = 0; currentDay <= dayIndex; currentDay++)
            {
                if (currentDay > 0 && firstDayOfWeek == currentDayOfWeek)
                {
                    currentWeek++;
                    if (currentWeek == weekCount) currentWeek = 0;
                }

                //jump out if we have reached the correct day
                if (currentDay == dayIndex)
                {
                    ret = weeks[currentWeek].IsWorkday(currentDayOfWeek);
                    break;
                }

                int nextDayOfWeek = (int)currentDayOfWeek + 1;
                if (nextDayOfWeek > 6) nextDayOfWeek -= 7;

                currentDayOfWeek = (DayOfWeek)nextDayOfWeek;
            }

            return ret;
        }

        public virtual bool IsWorkday(DateOnly owningPeriodStartDate, DateOnly requestedDate)
        {
            if (_contractScheduleWeeks.Count == 0)
                return true;

            DayOfWeek startDayOfWeek = owningPeriodStartDate.DayOfWeek;
            int startOffset = (int) DayOfWeek.Sunday - (int) startDayOfWeek;

            DateOnly contractScheduleStartDate = owningPeriodStartDate.AddDays(startOffset);
            TimeSpan dateOffset = requestedDate.Date.Subtract(contractScheduleStartDate);
            int totalDays = (int) dateOffset.TotalDays;
            int weekNo = ((totalDays -1) / 7);
            int weekIndex = weekNo % ContractScheduleWeeks.Count();
            int reminderDays = totalDays % 7;

            foreach (var contractScheduleWeek in _contractScheduleWeeks)
            {
                if (contractScheduleWeek.WeekOrder == weekIndex)
                {
                    return contractScheduleWeek.IsWorkday((DayOfWeek) reminderDays);
                }
            }

            throw new ArgumentException("Could not resolve requestedDate");
        }

        /// <summary>
        /// Resets the WeekOrder property of ContractScheduleWeeks upon deleting a one Week.
        /// </summary>
        private void resetWeekOrdersUponDeletion()
        {
            var i = 0;
            foreach (var contractScheduleWeek in ContractScheduleWeeks)
            {
                contractScheduleWeek.WeekOrder = i;
                i++;
            }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}