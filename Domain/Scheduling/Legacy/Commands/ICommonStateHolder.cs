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
		IEnumerable<IShiftCategory> ShiftCategories { get; }
	    IEnumerable<IScheduleTag> ScheduleTags { get; }
	    IDayOffTemplate DefaultDayOffTemplate { get; }
		ICollection<IWorkflowControlSet> WorkflowControlSets { get; }
		ICollection<IWorkflowControlSet> ModifiedWorkflowControlSets { get; }
		IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets { get; }
	    void SetDayOffTemplate(IDayOffTemplate dayOffTemplate);
		void SetShiftCategories(IList<IShiftCategory> shiftCategories);
	}
}