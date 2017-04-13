using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
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