using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public class AlarmEventArgs : EventArgs
    {
        public AlarmEventArgs(IJob job)
        {
            Job = job;
        }

        public IJob Job { get; set; }
    }
}