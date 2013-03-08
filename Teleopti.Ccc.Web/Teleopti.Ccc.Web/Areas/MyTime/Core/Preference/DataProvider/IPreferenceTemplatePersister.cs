using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceTemplatePersister
	{
		PreferenceTemplateViewModel Persist(PreferenceTemplateInput input);
		void Delete(Guid templateId);
	}
}