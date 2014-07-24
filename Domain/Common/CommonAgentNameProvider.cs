using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface ICommonAgentNameProvider
	{
		ICommonNameDescriptionSetting CommonAgentNameSettings { get; }
	}
	public class CommonAgentNameProvider : ICommonAgentNameProvider
	{
		private readonly IGlobalSettingDataRepository _settingDataRepository;
		private ICommonNameDescriptionSetting _commonNameDescription;

		public CommonAgentNameProvider(IGlobalSettingDataRepository settingDataRepository)
		{
			_settingDataRepository = settingDataRepository;
		}

		public ICommonNameDescriptionSetting CommonAgentNameSettings
		{
			get
			{
				if (_commonNameDescription == null)
				{
					_commonNameDescription = _settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
				}
				return _commonNameDescription;
			}
		}
	}
}