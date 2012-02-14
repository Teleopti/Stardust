﻿using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class PersonAvailability : AggregateRootWithBusinessUnit, IPersonAvailability, IDeleteTag 
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
            InParameter.NotNull("person", person);
            InParameter.NotNull("availability", availability);
            InParameter.NotNull("startDate", startDate);

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

        public virtual IPerson Person
        {
            get { return _person; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

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
                ICccTimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
                return timeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(StartDate.Date, DateTimeKind.Unspecified),
                                                     timeZoneInfo);
            }
        }

        public virtual IAvailabilityDay GetAvailabilityDay(DateOnly currentDate)
        {
            if (currentDate < _startDate.Date)
                throw new ArgumentOutOfRangeException("currentDate", currentDate, "Date must not be less than start date");
            
            int dateDiff = currentDate.Date.Subtract(_startDate.Date).Days;
            return _availability.FindAvailabilityDay(dateDiff + StartDay);
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
        
    }
}
