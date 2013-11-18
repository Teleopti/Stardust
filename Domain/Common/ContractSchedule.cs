using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using InParameter=Teleopti.Interfaces.Domain.InParameter;

namespace Teleopti.Ccc.Domain.Common
{

    public class ContractSchedule : VersionedAggregateRootWithBusinessUnit, IContractSchedule, IDeleteTag
    {
        private Description _description;
		private readonly Iesi.Collections.Generic.ISet<IContractScheduleWeek> _contractScheduleWeeks; //byt till BCL's ISet<T> när vi går över till .net 4.0!
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

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual bool IsWorkday(DateOnly personPeriodStartDate, DateOnly requestedDate)
		{
			if (_contractScheduleWeeks.Count == 0)
			    return true;

			if(requestedDate < personPeriodStartDate)
				throw new ArgumentException("Requested date is earlier than the person period's start date");

			BitArray contractWorkdaysList = createContractWorkdaysList(); // can be made in the constructor

			DayOfWeek periodStartDayOfWeek = personPeriodStartDate.DayOfWeek;
			int weekStartOffset = calculateWeekStartOffset(periodStartDayOfWeek);

			int daysBetweenPeriodStartAndRequested = daysBetweenDates(personPeriodStartDate, requestedDate);
			int daysBetweenPeriodStartAndRequestedAdjustedWithWeekStartOffset = weekStartOffset + daysBetweenPeriodStartAndRequested;
			
			int schemaTotalLength = schemaLength();

			int indexWithinInContractList =
				daysBetweenPeriodStartAndRequestedAdjustedWithWeekStartOffset % schemaTotalLength;
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

		/// <summary>
		/// Creates the contract workdays list.
		/// </summary>
		/// <returns></returns>
		private BitArray createContractWorkdaysList()
		{
			int bitArrayLength = schemaLength();
			int bitArrayCounter = 0;
			BitArray result = new BitArray(bitArrayLength);

			var weeks = _contractScheduleWeeks.OrderBy(w => w.WeekOrder).ToList();

			for (int weekIndex = 0; weekIndex < _contractScheduleWeeks.Count; weekIndex++)
			{
				IContractScheduleWeek currentWeek = weeks[weekIndex];

				for (int dayIndexInWeek = 0; dayIndexInWeek < 7; dayIndexInWeek++)
				{
					DayOfWeek currentDay = getCorrectDayOfWeek(dayIndexInWeek);
					result[bitArrayCounter] = currentWeek.IsWorkday(currentDay);
					bitArrayCounter++;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the correct day of week by the index that can be used to question the Contract week..
		/// </summary>
		/// <param name="dayIndexInWeek">The day index in week. Index 0 is always the start day in the person period week start day.</param>
		/// <param name="personWeekStartDay">The person week start day.</param>
		/// <returns></returns>
		private static DayOfWeek getCorrectDayOfWeek(int dayIndexInWeek)
		{
			int realIndex = (dayIndexInWeek + 1) % 7;
			DayOfWeek resultDay = (DayOfWeek)realIndex;
			return resultDay;
		}

    	/// <summary>
		/// Calculates the week start offset between the period start day and the contract start day.
		/// </summary>
		/// <param name="personWeekStartDay">The contract start day.</param>
		/// <returns></returns>
		private static int calculateWeekStartOffset(DayOfWeek personWeekStartDay)
		{
			const DayOfWeek periodStartDay = DayOfWeek.Monday; // it is always Monday as the start date
			int weekStartOffset =  (int)personWeekStartDay - (int)periodStartDay;
			int result = (weekStartOffset + 7) % 7;
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