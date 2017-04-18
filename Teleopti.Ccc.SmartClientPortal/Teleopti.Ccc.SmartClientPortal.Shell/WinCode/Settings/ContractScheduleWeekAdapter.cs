using System;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
    public class ContractScheduleWeekAdapter
    {
        private ContractScheduleWeek _containedEntity;

        public ContractScheduleWeekAdapter(ContractScheduleWeek containedEntity)
        {
            _containedEntity = containedEntity;
        }

        public ContractScheduleWeek ContainedEntity
        {
            get
            {
                return _containedEntity;
            }
        }

        public bool Sunday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Sunday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Sunday, isWorkingDay);
            }
        }

        public bool Monday 
        { 
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Monday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Monday , isWorkingDay);
            }
        }

        public bool Tuesday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Tuesday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Tuesday, isWorkingDay);
            }
        }

        public bool Wednesday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Wednesday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Wednesday, isWorkingDay);
            }
        }

        public bool Thursday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Thursday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Thursday, isWorkingDay);
            }
        }

        public bool Friday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Friday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Friday, isWorkingDay);
            }
        }

        public bool Saturday
        {
            get
            {
                return _containedEntity.IsWorkday(DayOfWeek.Saturday);
            }
            set
            {
                bool isWorkingDay = value;
                _containedEntity.Add(DayOfWeek.Saturday, isWorkingDay);
            }
        }
    }
}
