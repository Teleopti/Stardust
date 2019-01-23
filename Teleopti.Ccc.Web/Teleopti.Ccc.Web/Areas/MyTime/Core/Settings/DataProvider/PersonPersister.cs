using System.Globalization;
using MbCache.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public class PersonPersister : IPersonPersister
	{
		public void InvalidateCachedCulture(IPerson person)
		{
		}

		public void UpdateCulture(IPerson person, CultureInfo culture)
		{
			person.PermissionInformation.SetCulture(culture);
		}

		public void UpdateUICulture(IPerson person, CultureInfo culture)
		{
			person.PermissionInformation.SetUICulture(culture);
		}
	}
}