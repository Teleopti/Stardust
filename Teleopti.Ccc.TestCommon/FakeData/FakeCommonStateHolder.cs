using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCommonStateHolder : ICommonStateHolder
	{
		public void LoadCommonStateHolder(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
		{

		}

		public IEnumerable<IAbsence> Absences { get; private set; }
		public IEnumerable<IDayOffTemplate> DayOffs { get; private set; }
		public IEnumerable<IActivity> Activities { get; private set; }
		public IEnumerable<IActivity> ActiveActivities { get; private set; }
		public IEnumerable<IShiftCategory> ShiftCategories { get; private set; }
		public IEnumerable<IScheduleTag> ScheduleTags { get; private set; }
		public IEnumerable<IScheduleTag> ActiveScheduleTags { get; private set; }
		public IEnumerable<IAbsence> ActiveAbsences { get; private set; }
		public IEnumerable<IDayOffTemplate> ActiveDayOffs { get; private set; }
		public IEnumerable<IShiftCategory> ActiveShiftCategories { get; private set; }
		public IDayOffTemplate DefaultDayOffTemplate { get; private set; }
		public ICollection<IWorkflowControlSet> WorkflowControlSets { get; private set; }
		public ICollection<IWorkflowControlSet> ModifiedWorkflowControlSets { get; private set; }
		public IEnumerable<IPartTimePercentage> PartTimePercentages { get; private set; }
	}
}