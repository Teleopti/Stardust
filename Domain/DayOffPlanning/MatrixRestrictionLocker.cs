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
	    /// <param name="schedulingOptions">The scheduling options.</param>
	    /// <returns></returns>
	    IList<DateOnly> Execute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions);
    }

    public class MatrixRestrictionLocker : IMatrixRestrictionLocker
    {
        private readonly IRestrictionExtractor _extractor;

        public MatrixRestrictionLocker(IRestrictionExtractor extractor)
        {
            _extractor = extractor;
        }

        public IList<DateOnly> Execute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            var ret = new List<DateOnly>();
            foreach (IScheduleDayPro scheduleDayPro in matrix.UnlockedDays)
            {
                IScheduleDay daySchedulePart = scheduleDayPro.DaySchedulePart();
                var result = _extractor.Extract(daySchedulePart);
                SchedulePartView significantPart = daySchedulePart.SignificantPart();

                if (schedulingOptions.UseRotations)
                {
                    if(lockRotations(significantPart,result))
                        ret.Add(scheduleDayPro.Day);
                }

                if (schedulingOptions.UseAvailability)
                {
                    if (lockAvailability(significantPart, result))
                        ret.Add(scheduleDayPro.Day);
                }

                if (schedulingOptions.UsePreferencesMustHaveOnly)
                {
                    if (lockPreferences(significantPart, result, true))
                        ret.Add(scheduleDayPro.Day);

                }
                else
                {
                    if (schedulingOptions.UsePreferences)
                    {
                        if (lockPreferences(significantPart, result, false))
                            ret.Add(scheduleDayPro.Day);
                    }
                }
                
            }
            return ret;
        }

        private bool lockRotations(SchedulePartView significantPart, IExtractedRestrictionResult extractedRestrictionResult)
        {
            IList<IRotationRestriction> list = new List<IRotationRestriction>(extractedRestrictionResult.RotationList);
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

		private bool lockAvailability(SchedulePartView significantPart, IExtractedRestrictionResult extractedRestrictionResult)
        {
            IList<IAvailabilityRestriction> list = new List<IAvailabilityRestriction>(extractedRestrictionResult.AvailabilityList);
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

		private bool lockPreferences(SchedulePartView significantPart, IExtractedRestrictionResult extractedRestrictionResult, bool mustHaveOnly)
        {
            IList<IPreferenceRestriction> list = new List<IPreferenceRestriction>(extractedRestrictionResult.PreferenceList);
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