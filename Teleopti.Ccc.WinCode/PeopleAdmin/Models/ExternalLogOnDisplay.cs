using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public interface IExternalLogOnDisplay
    {
        string Description(IExternalLogOn externalLogOn);
    }

    public class ExternalLogOnDisplay : IExternalLogOnDisplay
    {
        public string Description(IExternalLogOn externalLogOn)
        {
			return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", externalLogOn.AcdLogOnName, externalLogOn.DataSourceName );
        }
		
    }
}