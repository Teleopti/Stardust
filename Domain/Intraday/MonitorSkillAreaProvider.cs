using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorSkillAreaProvider
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly IIntradayMonitorDataLoader _intradayMonitorDataLoader;

		public MonitorSkillAreaProvider(ISkillAreaRepository skillAreaRepository, IIntradayMonitorDataLoader intradayMonitorDataLoader)
		{
			_skillAreaRepository = skillAreaRepository;
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
		}

		public MonitorDataViewModel Load(Guid skillAreaId)
		{
			var skillArea = _skillAreaRepository.Get(skillAreaId);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return _intradayMonitorDataLoader.Load(skillIdList, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, DateOnly.Today);
		}
	}
}