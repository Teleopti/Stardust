using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class PersonRotation : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IPersonRotation, IDeleteTag 
    {
        private readonly IPerson _person;
        private IRotation _rotation;
        private DateOnly _startDate;
        private int _startDay;
        private bool _isDeleted;

        public PersonRotation(IPerson person, IRotation rotation, DateOnly startDate, int startDay)
        {
            InParameter.NotNull(nameof(person), person);
            InParameter.NotNull(nameof(rotation), rotation);
            InParameter.ValueMustBePositive(nameof(startDay), startDay);

            _person = person;
            _rotation = rotation;
            _startDate = startDate;
            _startDay = startDay;
        }

        protected PersonRotation()
        {
        }

        public virtual int StartDay
        {
            get { return _startDay; }
            set { _startDay = value;}
        }

        public virtual DateOnly StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public virtual DateTime StartDateAsUtc
        {
            get
            {
                TimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
                return timeZoneInfo.SafeConvertTimeToUtc(DateTime.SpecifyKind(StartDate.Date, DateTimeKind.Unspecified));
            }
        }

        public virtual IRotation Rotation
        {
            get { return _rotation; }
            set { _rotation = value;}
        }

        public virtual IPerson Person => _person;

	    public virtual bool IsDeleted => _isDeleted;

	    public virtual IRotationDay GetRotationDay(DateOnly currentDate)
        {
            if (currentDate < _startDate)
                throw new ArgumentOutOfRangeException(nameof(currentDate), currentDate, "Date must not be less than start date");
            
            int dateDiff = currentDate.Date.Subtract(_startDate.Date).Days;
            return _rotation.FindRotationDay(dateDiff + StartDay);
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}