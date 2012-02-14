using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public interface IPersonAbsenceAccountProvider
    {
        IPersonAccountCollection Find(IPerson person);
    }
}