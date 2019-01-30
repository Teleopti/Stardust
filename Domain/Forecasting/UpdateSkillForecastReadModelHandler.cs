using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	[EnabledBy(Toggles.WFM_Forecast_Readmodel_80790)]
	[InstancePerLifetimeScope]
	public class UpdateSkillForecastReadModelHandler : IHandleEvent<ForecastChangedEvent>, IRunOnStardust
	{

		private readonly SkillForecastIntervalCalculator _skillForecastIntervalCalculator;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;

		public UpdateSkillForecastReadModelHandler(SkillForecastIntervalCalculator skillForecastIntervalCalculator, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_skillForecastIntervalCalculator = skillForecastIntervalCalculator;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ForecastChangedEvent @event)
		{
			//may be this could be improved may be just call the new Skill SkillDayCalculator inthe loadSkillDays directly
			//could be optimized
			//var justSkillDays = _skillDayRepository.LoadSkillDays(@event.SkillDayIds);
			//var skills = justSkillDays.Select(x => x.Skill);
			//var period = new DateOnlyPeriod(justSkillDays.Min(x => x.CurrentDate),justSkillDays.Max(x => x.CurrentDate));
			//var allSkillDays = _skillDayRepository.FindReadOnlyRange(period, skills, _currentScenario.Current());
			_skillForecastIntervalCalculator.Calculate(@event.SkillDayIds);
		}
	}
}