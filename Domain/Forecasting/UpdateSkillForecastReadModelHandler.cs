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
	public class UpdateSkillForecastReadModelHandler : IHandleEvent<ForecastChangedEvent>, IHandleEvent<UpdateSkillForecastReadModelEvent>, IRunOnStardust
	{

		private readonly SkillForecastIntervalCalculator _skillForecastIntervalCalculator;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private ISkillRepository _skillRepository;

		private readonly SkillForecastReadModelPeriodBuilder _skillForecastReadModelPeriodBuilder;

		public UpdateSkillForecastReadModelHandler(SkillForecastIntervalCalculator skillForecastIntervalCalculator, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, SkillForecastReadModelPeriodBuilder skillForecastReadModelPeriodBuilder, ISkillRepository skillRepository)
		{
			_skillForecastIntervalCalculator = skillForecastIntervalCalculator;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_skillForecastReadModelPeriodBuilder = skillForecastReadModelPeriodBuilder;
			_skillRepository = skillRepository;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ForecastChangedEvent @event)
		{
			var justSkillDays = filterSkillDays(@event.SkillDayIds);
			if (!justSkillDays.Any()) return;
			var skills = justSkillDays.Select(x => x.Skill);
			var period = new DateOnlyPeriod(justSkillDays.Min(x => x.CurrentDate), justSkillDays.Max(x => x.CurrentDate));
			var skillDays = _skillDayRepository.FindReadOnlyRange(period, skills, _currentScenario.Current());
			_skillForecastIntervalCalculator.Calculate(skillDays);
		}


		private IEnumerable<ISkillDay> filterSkillDays(IEnumerable<Guid> skillDayIds)
		{
			var skillDays = _skillDayRepository.LoadSkillDays(skillDayIds);
			var validPeriod = _skillForecastReadModelPeriodBuilder.BuildFullPeriod();
			return skillDays.Where(x => x.CurrentDate.Date >= validPeriod.StartDateTime && x.CurrentDate.Date <= validPeriod.EndDateTime);
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateSkillForecastReadModelEvent @event)
		{
			var skills = _skillRepository.LoadAllSkills();
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(new DateOnly(@event.StartDateTime), new DateOnly(@event.EndDateTime)), skills, _currentScenario.Current());
			_skillForecastIntervalCalculator.Calculate(skillDays);
		}
	}
}