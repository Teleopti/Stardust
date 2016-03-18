using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
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
