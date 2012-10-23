using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class WorkTimeMinMaxRestrictionCreator : IWorkTimeMinMaxRestrictionCreator
	{
		private readonly IEffectiveRestrictionForDisplayCreator _effectiveRestrictionForDisplayCreator;

		public WorkTimeMinMaxRestrictionCreator(IEffectiveRestrictionForDisplayCreator effectiveRestrictionForDisplayCreator)
		{
			_effectiveRestrictionForDisplayCreator = effectiveRestrictionForDisplayCreator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public WorkTimeMinMaxRestrictionCreationResult MakeWorkTimeMinMaxRestriction(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions)
		{
			var result = new WorkTimeMinMaxRestrictionCreationResult();

			var effectiveRestriction = _effectiveRestrictionForDisplayCreator.MakeEffectiveRestriction(scheduleDay, effectiveRestrictionOptions);
			
			if (effectiveRestriction != null && effectiveRestriction.Absence != null)
				result.IsAbsenceInContractTime = effectiveRestriction.Absence.InContractTime;

			var meetings = scheduleDay.PersonMeetingCollection();
			if (meetings != null && meetings.Any())
				result.Restriction = new CombinedRestriction(effectiveRestriction, new MeetingRestriction(meetings));
			else
				result.Restriction = effectiveRestriction;

			return result;
		}
	}
}