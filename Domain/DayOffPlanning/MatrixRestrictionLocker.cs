using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{

    /// <summary>
    /// Locks the days of
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>Rotations,</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>Availability,</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>Preferences</description>
    /// 	</item>
    /// </list>
    /// </summary>
    public interface IMatrixRestrictionLocker
    {
        /// <summary>
        /// Returns the days with restrictions
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns></returns>
        IList<DateOnly> Execute(IScheduleMatrixPro matrix);
    }

    public class MatrixRestrictionLocker : IMatrixRestrictionLocker
    {
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IRestrictionExtractor _extractor;

        public MatrixRestrictionLocker(ISchedulingOptions schedulingOptions, 
            IRestrictionExtractor extractor)
        {
            _schedulingOptions = schedulingOptions;
            _extractor = extractor;
        }

        public IList<DateOnly> Execute(IScheduleMatrixPro matrix)
        {
            List<DateOnly> ret = new List<DateOnly>();
            foreach (IScheduleDayPro scheduleDayPro in matrix.UnlockedDays)
            {
                IScheduleDay daySchedulePart = scheduleDayPro.DaySchedulePart();
                _extractor.Extract(daySchedulePart);
                SchedulePartView significantPart = daySchedulePart.SignificantPart();

                if (_schedulingOptions.UseRotations)
                {
                    if(lockRotations(significantPart))
                        ret.Add(scheduleDayPro.Day);
                }

                if (_schedulingOptions.UseAvailability)
                {
                    if (lockAvailability(significantPart))
                        ret.Add(scheduleDayPro.Day);
                }

                if (_schedulingOptions.UsePreferencesMustHaveOnly)
                {
                    if (lockPreferences(significantPart, true))
                        ret.Add(scheduleDayPro.Day);

                }
                else
                {
                    if (_schedulingOptions.UsePreferences)
                    {
                        if (lockPreferences(significantPart, false))
                            ret.Add(scheduleDayPro.Day);
                    }
                }
                
            }
            return ret;
        }

        private bool lockRotations(SchedulePartView significantPart)
        {
            IList<IRotationRestriction> list = new List<IRotationRestriction>(_extractor.RotationList);
            if (list.Count > 0)
            {
                IRotationRestriction restriction = list[0];
                if (restriction.IsRestriction())
                {
					return shouldBeAndAreDayOff(restriction.DayOffTemplate != null, significantPart);
                }
            }
            return false;
        }

        private bool lockAvailability(SchedulePartView significantPart)
        {
            IList<IAvailabilityRestriction> list = new List<IAvailabilityRestriction>(_extractor.AvailabilityList);
            if (list.Count > 0)
            {
                IAvailabilityRestriction restriction = list[0];
                if (restriction.IsRestriction())
                {
                    if (restriction.NotAvailable)
                    {
                        if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.ContractDayOff)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool lockPreferences(SchedulePartView significantPart, bool mustHaveOnly)
        {
            IList<IPreferenceRestriction> list = new List<IPreferenceRestriction>(_extractor.PreferenceList);
            if (list.Count > 0)
            {
                IPreferenceRestriction restriction = list[0];
                if (restriction.IsRestriction())
                {
                    if (!restriction.MustHave && mustHaveOnly)
                    {
                        return false;
                    }

                	return shouldBeAndAreDayOff(restriction.DayOffTemplate != null, significantPart);
                }
            }
            return false;
        }

		private static bool shouldBeAndAreDayOff(bool shouldBeDayOff, SchedulePartView significantPart)
		{
			if (shouldBeDayOff)
			{
				if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.ContractDayOff)
					return true;
			}
			else
			{
				if (significantPart != SchedulePartView.DayOff && significantPart != SchedulePartView.ContractDayOff)
					return true;
			}
			return false;
		}
    }
}