using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class PersonAvailability : VersionedAggregateRootWithBusinessUnit, IPersonAvailability, IDeleteTag 
    {
        private readonly IPerson _person;
        private IAvailabilityRotation _availability;
        private DateOnly _startDate;
        private bool _isDeleted;
        private int _startDay;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailability"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="availability">The availability.</param>
        /// <param name="startDate">The start date.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        public PersonAvailability(IPerson person, IAvailabilityRotation availability, DateOnly startDate) 
        {
            InParameter.NotNull(nameof(person), person);
            InParameter.NotNull(nameof(availability), availability);
            InParameter.NotNull(nameof(startDate), startDate);

            _person = person;
            _availability = availability;
            _startDate = startDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailability"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        protected PersonAvailability()
        {
        }

        #region IPersonAvailability Members

        public virtual int StartDay
        {
            get { return _startDay; }
            set { _startDay = value; }
        }

        public virtual DateOnly StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public virtual IPerson Person => _person;

	    public virtual bool IsDeleted => _isDeleted;

	    public virtual IAvailabilityRotation Availability
        {
            get
            {
                return _availability;
            }
            set
            {
                _availability = value;
            }
        }


        public virtual DateTime StartDateAsUtc
        {
            get
            {
                TimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
                return timeZoneInfo.SafeConvertTimeToUtc(DateTime.SpecifyKind(StartDate.Date, DateTimeKind.Unspecified));
            }
        }

        public virtual IAvailabilityDay GetAvailabilityDay(DateOnly currentDate)
        {
            if (currentDate < _startDate)
                throw new ArgumentOutOfRangeException(nameof(currentDate), currentDate, "Date must not be less than start date");
            
            int dateDiff = currentDate.Subtract(_startDate).Days;
            return _availability.FindAvailabilityDay(dateDiff + StartDay);
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
        
    }
}
