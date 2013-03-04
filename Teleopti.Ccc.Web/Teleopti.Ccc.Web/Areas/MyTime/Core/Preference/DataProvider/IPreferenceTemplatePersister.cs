using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceTemplatePersister
	{
		void Persist(PreferenceTemplateInput input);
	}
}