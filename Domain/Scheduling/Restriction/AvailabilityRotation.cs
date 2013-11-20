using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    
    public class AvailabilityRotation : VersionedAggregateRootWithBusinessUnit, IAvailabilityRotation, IDeleteTag
    {
        private string _name;
        private readonly IList<IAvailabilityDay> _availabilityDays;
        private bool _isDeleted;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AvailabilityRotation(string name, int countDays)
        {
            _name = name;
            _availabilityDays = new List<IAvailabilityDay>();
            AddDays(countDays);
        }

        protected AvailabilityRotation()
        {
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<IAvailabilityDay> AvailabilityDays
        {
            get { return _availabilityDays; }
        }

        public virtual int DaysCount
        {
            get { return AvailabilityDays.Count; }
        }


        public virtual IAvailabilityDay FindAvailabilityDay(int dayCount)
        {
            InParameter.ValueMustBePositive("dayCount", dayCount);

            int overflow;
            Math.DivRem(dayCount, DaysCount, out overflow);

            return AvailabilityDays[overflow];
        }

        public virtual void AddDays(int dayCount)
        {
            for (int i = 0; i < dayCount; i++)
            {
                AvailabilityDay newDay = new AvailabilityDay();
                newDay.SetParent(this);
                _availabilityDays.Add(newDay);
            }
        }

        public virtual void RemoveDays(int dayCount)
        {
            if (dayCount > _availabilityDays.Count)
                throw new ArgumentOutOfRangeException("dayCount", dayCount, "The parameter can not be greater than the number of days in the Rotation");
            for (int day = 0; day < dayCount; day++)
            {
                _availabilityDays.RemoveAt(_availabilityDays.Count - 1);
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual bool IsChoosable
        {
            get { return !IsDeleted; }
        }
    }
}