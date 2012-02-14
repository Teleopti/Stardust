using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Day off planner rules.
    /// </summary>
    public class DayOffPlannerRules : IDayOffPlannerRules
    {
        private bool _useDaysOffPerWeek = true;
        private MinMax<int> _daysOffPerWeek = new MinMax<int>(1, 3);

        private bool _useConsecutiveDaysOff = true;
        private MinMax<int> _consecutiveDaysOff = new MinMax<int>(1, 3);

        private bool _useConsecutiveWorkdays = true;
        private MinMax<int> _consecutiveWorkdays = new MinMax<int>(2, 6);

        //ej
        private bool _usePreWeek;
        private bool _usePostWeek;

        //ej
        private string _name;
        private int _numberOfDaysInPeriod;
        private int _numberOfDayOffsInPeriod;

        //använd
        private bool _useFreeWeekends;
        private bool _useFreeWeekendDays;
        
        //ej
        private MinMax<int> _freeWeekends;
        private MinMax<int> _freeWeekendDays;

        private bool _keepWeekendsTogether;
        private bool _keepFreeWeekends;
        private bool _keepFreeWeekendDays;

        //for use only on reoptimize
        private int _maximumMoveDaysPerPerson;

        public virtual int NumberOfDayOffsInPeriod
        {
            get { return _numberOfDayOffsInPeriod; }
            set { _numberOfDayOffsInPeriod = value; }
        }

        public virtual int NumberOfDaysInPeriod
        {
            get { return _numberOfDaysInPeriod; }
            set { _numberOfDaysInPeriod = value; }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual bool UseConsecutiveWorkdays
        {
            get { return _useConsecutiveWorkdays; }
            set { _useConsecutiveWorkdays = value; }
        }

        public virtual MinMax<int> ConsecutiveWorkdays
        {
            get { return _consecutiveWorkdays; }
            set { _consecutiveWorkdays = value; }
        }

        public virtual bool UseDaysOffPerWeek
        {
            get { return _useDaysOffPerWeek; }
            set { _useDaysOffPerWeek = value; }
        }

        public virtual MinMax<int> DaysOffPerWeek
        {
            get { return _daysOffPerWeek; }
            set { _daysOffPerWeek = value; }
        }

        public virtual bool UseConsecutiveDaysOff
        {
            get { return _useConsecutiveDaysOff; }
            set { _useConsecutiveDaysOff = value; }
        }

        public virtual MinMax<int> ConsecutiveDaysOff
        {
            get { return _consecutiveDaysOff; }
            set { _consecutiveDaysOff = value; }
        }

        public virtual bool UsePreWeek
        {
            get { return _usePreWeek; }
            set { _usePreWeek = value; }
        }

        public virtual bool UsePostWeek
        {
            get { return _usePostWeek; }
            set { _usePostWeek = value; }
        }

        public virtual bool UseFreeWeekends
        {
            get { return _useFreeWeekends; }
            set { _useFreeWeekends = value; }
        }

        public virtual bool UseFreeWeekendDays
        {
            get { return _useFreeWeekendDays; }
            set { _useFreeWeekendDays = value; }
        }

        public virtual MinMax<int> FreeWeekends
        {
            get { return _freeWeekends; }
            set { _freeWeekends = value; }
        }

        public virtual MinMax<int> FreeWeekendDays
        {
            get { return _freeWeekendDays; }
            set { _freeWeekendDays = value; }
        }

        public virtual int MaximumMovableDayOffsPerPerson
        {
            get { return _maximumMoveDaysPerPerson; }
            set { _maximumMoveDaysPerPerson = value; }
        }

        public virtual bool UseMoveMaxDays
        {
            get { return (_maximumMoveDaysPerPerson > 0); }
        }

        public virtual bool KeepWeekendsTogether
        {
            get { return _keepWeekendsTogether; }
            set { _keepWeekendsTogether = value; }
        }

        public virtual bool KeepFreeWeekends
        {
            get { return _keepFreeWeekends; }
            set { _keepFreeWeekends = value; }
        }

        public virtual bool KeepFreeWeekendDays
        {
            get { return _keepFreeWeekendDays; }
            set { _keepFreeWeekendDays = value; }
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
