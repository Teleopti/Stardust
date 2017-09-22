using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public interface IPersonPersister
	{
		void UpdateCulture(IPerson person, CultureInfo culture);
		void UpdateUICulture(IPerson person, CultureInfo culture);
	}
}