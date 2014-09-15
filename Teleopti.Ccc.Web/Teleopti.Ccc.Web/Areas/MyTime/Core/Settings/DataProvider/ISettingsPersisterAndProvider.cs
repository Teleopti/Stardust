using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public interface ISettingsPersisterAndProvider<T>
	{

		T Persist(T isActive);
		T Get();
		T GetByOwner(IPerson person);

	}
}