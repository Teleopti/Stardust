using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using InParameter=Teleopti.Ccc.Domain.InterfaceLegacy.Domain.InParameter;

namespace Teleopti.Ccc.Domain.Common
{

    public class ContractSchedule : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IContractSchedule, IDeleteTag
    {
        private Description _description;
	    private readonly ISet<IContractScheduleWeek> _contractScheduleWeeks;
        private bool _isDeleted;

        /// <summary>
        /// Creates a new instance of ContractSchedule
        /// </summary>
        /// <param name="name">Name of ContractSchedule</param>
        public ContractSchedule(string name)
        {
            _description = new Description(name);
	        _contractScheduleWeeks = new HashSet<IContractScheduleWeek>();
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
        public virtual IEnumerable<IContractScheduleWeek> ContractScheduleWeeks => _contractScheduleWeeks;

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
            InParameter.NotNull(nameof(contractScheduleWeek), contractScheduleWeek);
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
            InParameter.NotNull(nameof(contractScheduleWeek), contractScheduleWeek);
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

	    public virtual bool IsDeleted => _isDeleted;

	    public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual bool IsWorkday(DateOnly personPeriodStartDate, DateOnly requestedDate, DayOfWeek dayOfWeek)
		{
			if (_contractScheduleWeeks.Count == 0) return true;
			if(requestedDate < personPeriodStartDate) throw new ArgumentException("Requested date is earlier than the person period's start date");
			var periodStartDayOfWeek = personPeriodStartDate.DayOfWeek;
			var weekStartOffset = calculateWeekStartOffset(periodStartDayOfWeek, dayOfWeek);
			var firstDayOfWeek =  (7 + (int)dayOfWeek - 1) % 7;
			var contractWorkdaysList = createContractWorkdaysList(firstDayOfWeek);
			var daysBetweenPeriodStartAndRequested = daysBetweenDates(personPeriodStartDate, requestedDate);
			var daysBetweenPeriodStartAndRequestedAdjustedWithWeekStartOffset = weekStartOffset + daysBetweenPeriodStartAndRequested;
			var indexWithinInContractList = daysBetweenPeriodStartAndRequestedAdjustedWithWeekStartOffset % schemaLength();
			
			return contractWorkdaysList[indexWithinInContractList];
		}

		private static int daysBetweenDates(DateOnly date1, DateOnly date2)
		{
			TimeSpan dateOffset = date2.Date.Subtract(date1.Date);
			return (int)dateOffset.TotalDays;
		}

    	private int schemaLength()
		{
			return _contractScheduleWeeks.Count*7;
		}

		
		private BitArray createContractWorkdaysList(int weekStartOffset)
		{
			var bitArrayLength = schemaLength();
			var bitArrayCounter = 0;
			var result = new BitArray(bitArrayLength);
			var weeks = _contractScheduleWeeks.OrderBy(w => w.WeekOrder).ToList();
			for (var weekIndex = 0; weekIndex < _contractScheduleWeeks.Count; weekIndex++)
			{
				var currentWeek = weeks[weekIndex];
				for (var dayIndexInWeek = 0; dayIndexInWeek < 7; dayIndexInWeek++)
				{
					var currentDay = getCorrectDayOfWeek(dayIndexInWeek, weekStartOffset);
					result[bitArrayCounter] = currentWeek.IsWorkday(currentDay);
					bitArrayCounter++;
				}
			}
			return result;
		}
		
		private static DayOfWeek getCorrectDayOfWeek(int dayIndexInWeek, int weekStartOffset)
		{
			var realIndex = (dayIndexInWeek + 1 + weekStartOffset) % 7;
			return (DayOfWeek)realIndex;
		}

		private static int calculateWeekStartOffset(DayOfWeek personWeekStartDay, DayOfWeek dayOfWeek)
		{
			var weekStartOffset =  (int)personWeekStartDay - (int)dayOfWeek;
			var result = (weekStartOffset + 7) % 7;
			return result;
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
    }
}