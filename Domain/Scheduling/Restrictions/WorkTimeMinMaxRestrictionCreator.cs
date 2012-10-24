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
			var personAssignments = scheduleDay.PersonAssignmentCollection();
			result.Restriction = effectiveRestriction;
			if (meetings != null && meetings.Any())
				result.Restriction = new CombinedRestriction(result.Restriction, new MeetingRestriction(meetings));
			if (personAssignments != null && personAssignments.Select(personAssignment => personAssignment.PersonalShiftCollection).Any(personalShifts => personalShifts.Any()))
				result.Restriction = new CombinedRestriction(result.Restriction, new PersonalShiftRestriction(personAssignments));
			return result;
		}
	}
}