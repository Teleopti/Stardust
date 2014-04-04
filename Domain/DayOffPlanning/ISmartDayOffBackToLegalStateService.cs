using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface ISmartDayOffBackToLegalStateService
    {
        IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray);

        bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations);

        IList<string> FailedSolverDescriptionKeys { get; }
    }
}