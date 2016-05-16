﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
    [DisabledBy(Toggles.Wfm_RecalculateForecastOnHangfire_37971)]
#pragma warning disable 618
    public class RecalculateForecastOnSkillEventHandlerOnServiceBus : RecalculateForecastOnSkillEventHandlerBase, IHandleEvent<RecalculateForecastOnSkillCollectionEvent>, IRunOnServiceBus
#pragma warning restore 618
    {
        public RecalculateForecastOnSkillEventHandlerOnServiceBus(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository, 
                                                                ISkillRepository skillRepository,  IStatisticLoader statisticLoader, 
                                                                IReforecastPercentCalculator reforecastPercentCalculator) 
                                                                : base(scenarioRepository, skillDayRepository, skillRepository,  statisticLoader, reforecastPercentCalculator)
        {
        }

        public new void Handle(RecalculateForecastOnSkillCollectionEvent @event)
        {
            base.Handle(@event);
        }
    }

    [EnabledBy(Toggles.Wfm_RecalculateForecastOnHangfire_37971)]
    public class RecalculateForecastOnSkillEventHandlerOnHangfire : RecalculateForecastOnSkillEventHandlerBase, IHandleEvent<RecalculateForecastOnSkillCollectionEvent>, IRunOnHangfire
    {
        public RecalculateForecastOnSkillEventHandlerOnHangfire(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository,
                                                                ISkillRepository skillRepository,IStatisticLoader statisticLoader,
                                                                IReforecastPercentCalculator reforecastPercentCalculator)
                                                                : base(scenarioRepository, skillDayRepository, skillRepository,  statisticLoader, reforecastPercentCalculator)
        {
        }
        [AsSystem, UnitOfWork]
        public new virtual void Handle(RecalculateForecastOnSkillCollectionEvent @event)
        {
            base.Handle(@event);
        }
    }

    public class RecalculateForecastOnSkillEventHandlerBase 
    {
        private readonly IReforecastPercentCalculator _reforecastPercentCalculator;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ISkillDayRepository _skillDayRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IStatisticLoader _statisticLoader;

	    public RecalculateForecastOnSkillEventHandlerBase(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository,
                                                      ISkillRepository skillRepository, IStatisticLoader statisticLoader, IReforecastPercentCalculator reforecastPercentCalculator)
        {
            _scenarioRepository = scenarioRepository;
            _skillDayRepository = skillDayRepository;
            _skillRepository = skillRepository;
	        _statisticLoader = statisticLoader;
            _reforecastPercentCalculator = reforecastPercentCalculator;
        }
        
        public void Handle(RecalculateForecastOnSkillCollectionEvent @event)
        {

                var scenario = _scenarioRepository.Get(@event.ScenarioId);
                if (!scenario.DefaultScenario)
                {
                    return;
                }

                foreach (var skillMessage in @event.SkillCollection)
                {
                    var dateOnly = DateOnly.Today;
                    var skill = _skillRepository.Get(skillMessage.SkillId);
                    if (skill == null)
                    {
                        continue;
                    }
                    var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
                    var period = new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(skill.TimeZone);
                    period = period.ChangeEndTime(skill.MidnightBreakOffset.Add(TimeSpan.FromHours(1)));

                    var skillDays = new List<ISkillDay>(_skillDayRepository.FindRange(dateOnlyPeriod, skill, scenario));
                    foreach (var skillDay in skillDays)
                    {
                        foreach (var workloadDay in skillDay.WorkloadDayCollection)
                        {
                            if (!skillMessage.WorkloadIds.Contains(workloadDay.Workload.Id.GetValueOrDefault()))
                            {
                                continue;
                            }
                            var lastInterval = _statisticLoader.Execute(period, workloadDay, skillDay.SkillStaffPeriodCollection);
                            var perc = _reforecastPercentCalculator.Calculate(workloadDay, lastInterval);
                            workloadDay.CampaignTasks = new Percent(0);
                            workloadDay.Tasks = workloadDay.Tasks*perc;
                        }
                    }
            }
        }
    }
}