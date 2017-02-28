using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity
{
    public interface IActivityModel : IDeleteTag
    {
        string Name { get; }
        IActivity Entity { get; }
    }
}