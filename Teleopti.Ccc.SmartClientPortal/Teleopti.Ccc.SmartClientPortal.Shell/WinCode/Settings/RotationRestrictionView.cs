using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
    /// <summary>
    /// Represents an enhanced view of restrictions during a rotation day.
    /// </summary>
    public class RotationRestrictionView : ScheduleRestrictionBaseView
    {
        private IDayOffTemplate _dayOffTemplate;
        private IShiftCategory _shiftCategory;
        private static IDayOffTemplate _defaultDayOff;
        private static IShiftCategory _defaultShiftCategory;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RotationRestrictionView"/> is a day off.
        /// </summary>
        /// <value><c>True</c> if day off; otherwise, <c>False</c>.</value>
        public IDayOffTemplate DayOffTemplate
        {
            get
            {
                return _dayOffTemplate ?? DefaultDayOff;
            }
            set
            {
                if (value == null || value == DefaultDayOff)
                    _dayOffTemplate = null;
                else
                    _dayOffTemplate = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IShiftCategory ShiftCategory
        {
            get
            {
                return _shiftCategory ?? DefaultShiftCategory;
            }
            set
            {
                if (value == null || value == DefaultShiftCategory)
                    _shiftCategory = null;
                else
                    _shiftCategory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static IShiftCategory DefaultShiftCategory
        {
            get
            {
                if (_defaultShiftCategory == null)
                {
                    _defaultShiftCategory = CreateShiftCategory();
                }
                return _defaultShiftCategory;
            }
        }

        public static IDayOffTemplate DefaultDayOff
        {
            get
            {
                if (_defaultDayOff == null)
                {
                    _defaultDayOff = CreateEmptyDayOff();
                }
                return _defaultDayOff;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationRestrictionView"/> class.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="week">A week number.</param>
        /// <param name="day">A day number.</param>
        public RotationRestrictionView(IRestrictionBase target, int week, int day)
            : base(target, week, day)
        {
        	var rotationRestriction = (IRotationRestriction) ContainedEntity;
			if (rotationRestriction.DayOffTemplate != null)
				_dayOffTemplate = ((DayOffTemplate)rotationRestriction.DayOffTemplate).EntityClone();

			if (rotationRestriction.ShiftCategory != null)
				_shiftCategory = ((ShiftCategory)rotationRestriction.ShiftCategory).EntityClone();
        }

        private static IShiftCategory CreateShiftCategory()
        {
            IShiftCategory newInstance = new ShiftCategory(Resources.SelectShiftCategory);
            return newInstance;
        }

        private static IDayOffTemplate CreateEmptyDayOff()
        {
            IDayOffTemplate newInstance = new DayOffTemplate(new Description(Resources.DayOffTemplateDefaultDescription));
            return newInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStartTimeChanged()
        {
            OnTimeChanged(EarlyStartTime, LateStartTime);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnEndTimeChanged()
        {
            OnTimeChanged(EarlyEndTime, LateEndTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minTime"></param>
        /// <param name="maxTime"></param>
        private void OnTimeChanged(TimeSpan? minTime, TimeSpan? maxTime)
        {
			if (minTime.HasValue || maxTime.HasValue)
				DayOffTemplate = null;
        }

        public void AssignValuesToDomainObject()
        {
			var rotationRestriction = (IRotationRestriction)ContainedEntity;
            if (_dayOffTemplate == null || _dayOffTemplate == DefaultDayOff)
                rotationRestriction.DayOffTemplate = null;
            else
                rotationRestriction.DayOffTemplate = ((DayOffTemplate) _dayOffTemplate).EntityClone();

            if (_shiftCategory == null || _shiftCategory == DefaultShiftCategory)
                rotationRestriction.ShiftCategory = null;
            else
                rotationRestriction.ShiftCategory = ((ShiftCategory)_shiftCategory).EntityClone();
            
            ContainedEntity.StartTimeLimitation = StartTimeLimit();
            ContainedEntity.EndTimeLimitation = EndTimeLimit();
            ContainedEntity.WorkTimeLimitation = WorkTimeLimit();
        }
    }
}
