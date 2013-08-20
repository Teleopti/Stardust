using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public interface IPersonPersister
	{
		void UpdateCulture(IPerson person, CultureInfo culture);
		void UpdateUICulture(IPerson person, CultureInfo culture);
	}
}