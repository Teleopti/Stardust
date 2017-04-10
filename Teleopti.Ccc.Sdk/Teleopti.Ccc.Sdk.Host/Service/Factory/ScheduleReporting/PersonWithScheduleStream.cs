using System.IO;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal class PersonWithScheduleStream
    {
        public IPerson Person { get; set; }
        public Stream SchedulePdf { get; set; }
    }
}