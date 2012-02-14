using System;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public class CustomEventArgs : EventArgs
    {
        private readonly IJobResult _jobResult;
        private readonly IJobStepResult _jobStepResult;

        public CustomEventArgs(IJobResult jobResult)
        {
            _jobResult = jobResult;
        }

        public CustomEventArgs(IJobStepResult jobStepResult)
        {
            _jobStepResult = jobStepResult;
        }

        public IJobResult JobResult
        {
            get { return _jobResult; }
        }

        public IJobStepResult JobStepResult
        {
            get { return _jobStepResult; }
        }
    }
}