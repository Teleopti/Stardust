using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceTemplateProvider
	{
		IEnumerable<IExtendedPreferenceTemplate> RetrievePreferenceTemplates();
	}
}