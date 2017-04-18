using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity
{
    public interface IActivityModel : IDeleteTag
    {
        string Name { get; }
        IActivity Entity { get; }
    }
}