using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
{
    public interface IRoleDisplay
    {
        string Description(IApplicationRole role);
    }

    public class RoleDisplay : IRoleDisplay
    {
        public string Description(IApplicationRole role)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}", role.DescriptionText);
        }
    }
}