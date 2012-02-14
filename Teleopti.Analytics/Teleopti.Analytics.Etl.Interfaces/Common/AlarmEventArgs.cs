using System;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Interfaces.Common
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