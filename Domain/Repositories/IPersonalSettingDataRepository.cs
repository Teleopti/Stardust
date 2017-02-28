using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonalSettingDataRepository : ISettingDataRepository
	{
		T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue)
			where T : class, ISettingValue;
	}
}