﻿using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
    public class RunningStatusRepository
    {
        private readonly IRunControllerRepository _repository;

        public RunningStatusRepository(string connectionString)
        {
            _repository = new Repository(connectionString);
        }

        public IEtlRunningInformation GetRunningJob()
        {
            IEtlRunningInformation etlRunningInformation;
            _repository.IsAnotherEtlRunningAJob(out etlRunningInformation);

            return etlRunningInformation;
        }
    }
}
