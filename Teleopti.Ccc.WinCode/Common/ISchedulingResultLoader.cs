﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
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
