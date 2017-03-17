using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface ISmartDayOffBackToLegalStateService
    {
		IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences, int maxIterations);

        bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations, ICollection<string> failedSolverDescriptionKeys);
    }
}