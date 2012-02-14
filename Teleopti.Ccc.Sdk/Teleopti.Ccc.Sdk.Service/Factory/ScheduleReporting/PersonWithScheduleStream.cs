using System.IO;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting
{
    internal class PersonWithScheduleStream
    {
        public IPerson Person { get; set; }
        public Stream SchedulePdf { get; set; }
    }
}