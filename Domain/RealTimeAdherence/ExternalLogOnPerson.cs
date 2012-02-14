using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class ExternalLogOnPerson
    {
        public IPerson Person { get; set; }

        public int DataSourceId { get; set; }

        public string ExternalLogOn { get; set; }
    }
}