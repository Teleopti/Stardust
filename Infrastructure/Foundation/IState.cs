using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public interface IState : IStateReader
    {
        void SetApplicationData(IApplicationData applicationData);
    }
}