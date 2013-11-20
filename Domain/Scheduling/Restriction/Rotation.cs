using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{

    public class Rotation : VersionedAggregateRootWithBusinessUnit, IDeleteTag, IRotation
    {
        private string _name;
        private readonly IList<IRotationDay> _rotationDays;
        private bool _isDeleted;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Rotation(string name, int countDays)
        {
            _name = name;
            _rotationDays = new List<IRotationDay>();
            AddDays(countDays);
        }

        protected Rotation()
        {
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IList<IRotationDay> RotationDays
        {
            get { return _rotationDays; }
        }

        public virtual int DaysCount
        {
            get { return RotationDays.Count; }
        }

        public virtual IRotationDay FindRotationDay(int dayCount)
        {
            InParameter.ValueMustBePositive("dayCount", dayCount);

            int overflow;
            Math.DivRem(dayCount, DaysCount, out overflow);

            return RotationDays[overflow];
        }

        public virtual void AddDays(int dayCount)
        {
            InParameter.ValueMustBeLargerThanZero("dayCount", dayCount);
            int countDays = _rotationDays.Count;
            for (int day = countDays; day < countDays + dayCount; day++)
            {
                RotationDay rotationDay = new RotationDay();
                rotationDay.SetParent(this);
                _rotationDays.Add(rotationDay);
            }
        }

        public virtual void RemoveDays(int dayCount)
        {
            if (dayCount > _rotationDays.Count)
                throw new ArgumentOutOfRangeException("dayCount", dayCount, "The parameter can not be greater than the number of days in the Rotation");
            for (int day = 0; day < dayCount; day++)
            {
                _rotationDays.RemoveAt(_rotationDays.Count - 1);
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
