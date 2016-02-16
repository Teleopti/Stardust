using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public interface IState : IStateReader
    {
        void SetApplicationData(IApplicationData applicationData);
    }
}