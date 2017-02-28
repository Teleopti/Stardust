﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
				_commonNameDescription = _settingDataRepository.FindValueByKey(CommonNameDescriptionSetting.Key,
					new CommonNameDescriptionSetting());
				return _commonNameDescription;
			}
		}
	}
}