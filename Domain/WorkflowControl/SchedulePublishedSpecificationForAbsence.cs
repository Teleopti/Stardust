using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	/// <summary>
	/// Check if the schedule is published for the schedule data is an Absence
	/// </summary>
	/// <remarks>
	/// This specification fixes the bug of 23072. 
	/// An absence layer can extend more than one day, so it can happen, that an absence
	/// starts before PreferencePeriod but has absence days in the PreferencePeriod. 
	/// </remarks>
	public class SchedulePublishedSpecificationForAbsence :
		Specification<PublishedScheduleData>
	{
		private readonly IWorkflowControlSet _workflowControlSet;
		private readonly ScheduleVisibleReasons _scheduleVisibleReasons;

		public SchedulePublishedSpecificationForAbsence(IWorkflowControlSet workflowControlSet,
		                                                ScheduleVisibleReasons scheduleVisibleReasons)
		{
			_workflowControlSet = workflowControlSet;
			_scheduleVisibleReasons = scheduleVisibleReasons;
		}

		public override bool IsSatisfiedBy(PublishedScheduleData obj)
		{
			if (_workflowControlSet == null) return false;
			if (obj.ScheduleData is IPersonAbsence)
			{
				if (_workflowControlSet.PreferencePeriod.StartDate > obj.SchedulingDate.DateOnly)
					return false;
				if ((_scheduleVisibleReasons & ScheduleVisibleReasons.Preference) == ScheduleVisibleReasons.Preference)
				{
					if (_workflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today) &&
					    _workflowControlSet.PreferencePeriod.Intersection(
						    obj.ScheduleData.Period.ToDateOnlyPeriod(obj.TimeZoneInfo)).HasValue)
						return true;
				}

			}
			return false;
		}
	}

	public class PublishedScheduleData
	{
		public PublishedScheduleData(
			IDateOnlyAsDateTimePeriod schedulingDate,
			IScheduleData scheduleData, 
			TimeZoneInfo timeZoneInfo)
		{
			SchedulingDate = schedulingDate;
			ScheduleData = scheduleData;
			TimeZoneInfo = timeZoneInfo;
		}

		public IDateOnlyAsDateTimePeriod SchedulingDate { get; private set; }
		public IScheduleData ScheduleData { get; private set; }
		public TimeZoneInfo TimeZoneInfo { get; private set; }
	}

}