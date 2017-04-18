using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface ISchedulingResultLoader
    {
        ISchedulerStateHolder SchedulerState { get; }

        void LoadWithIntradayData(IUnitOfWork unitOfWork);

        void ReloadForecastData(IUnitOfWork unitOfWork);

        IEnumerable<IContractSchedule> ContractSchedules { get; }

        IEnumerable<IContract> Contracts { get; }

        IEnumerable<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets { get; }

        void InitializeScheduleData();
        void ReloadScheduleData(IUnitOfWork unitOfWork);
    }
}
