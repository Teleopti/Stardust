using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface ICommonStateHolder
    {
        void LoadCommonStateHolder(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork);

		IEnumerable<IAbsence> Absences { get; }

		IEnumerable<IDayOffTemplate> DayOffs { get; }

		IEnumerable<IActivity> Activities { get; }

		IEnumerable<IActivity> ActiveActivities { get; }

		IEnumerable<IShiftCategory> ShiftCategories { get; }

	    IEnumerable<IScheduleTag> ScheduleTags { get; }
	    IEnumerable<IScheduleTag> ActiveScheduleTags { get; }
	    IEnumerable<IAbsence> ActiveAbsences { get; }
	    IEnumerable<IDayOffTemplate> ActiveDayOffs { get; }
	    IEnumerable<IShiftCategory> ActiveShiftCategories { get; }
	    IDayOffTemplate DefaultDayOffTemplate { get; }
		ICollection<IWorkflowControlSet> WorkflowControlSets { get; }
		ICollection<IWorkflowControlSet> ModifiedWorkflowControlSets { get; }

		IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets { get; }
	    void LoadCommonStateHolderForResourceCalculationOnly(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork);

	    void SetDayOffTemplate(IDayOffTemplate dayOffTemplate);
    }
}