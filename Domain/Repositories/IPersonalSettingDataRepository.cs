using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonalSettingDataRepository : ISettingDataRepository
	{
		T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue)
			where T : class, ISettingValue;
	}
}