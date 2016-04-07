using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
    [UseNotOnToggle(Toggles.Wfm_RecalculateForecastOnHangfire_37971)]
    public class RecalculateForecastOnSkillEventHandlerOnServiceBus : RecalculateForecastOnSkillEventHandlerBase, IHandleEvent<RecalculateForecastOnSkillCollectionEvent>, IRunOnServiceBus
    {
        public RecalculateForecastOnSkillEventHandlerOnServiceBus(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository, 
                                                                ISkillRepository skillRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IStatisticLoader statisticLoader, 
                                                                IReforecastPercentCalculator reforecastPercentCalculator) 
                                                                : base(scenarioRepository, skillDayRepository, skillRepository, unitOfWorkFactory, statisticLoader, reforecastPercentCalculator)
        {
        }

        public new void Handle(RecalculateForecastOnSkillCollectionEvent @event)
        {
            base.Handle(@event);
        }
    }

    [UseOnToggle(Toggles.Wfm_RecalculateForecastOnHangfire_37971)]
    public class RecalculateForecastOnSkillEventHandlerOnHangfire : RecalculateForecastOnSkillEventHandlerBase, IHandleEvent<RecalculateForecastOnSkillCollectionEvent>, IRunOnHangfire
    {
        public RecalculateForecastOnSkillEventHandlerOnHangfire(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository,
                                                                ISkillRepository skillRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IStatisticLoader statisticLoader,
                                                                IReforecastPercentCalculator reforecastPercentCalculator)
                                                                : base(scenarioRepository, skillDayRepository, skillRepository, unitOfWorkFactory, statisticLoader, reforecastPercentCalculator)
        {
        }
        [AsSystem]
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
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public RecalculateForecastOnSkillEventHandlerBase(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository,
                                                      ISkillRepository skillRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IStatisticLoader statisticLoader, IReforecastPercentCalculator reforecastPercentCalculator)
        {
            _scenarioRepository = scenarioRepository;
            _skillDayRepository = skillDayRepository;
            _skillRepository = skillRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _statisticLoader = statisticLoader;
            _reforecastPercentCalculator = reforecastPercentCalculator;
        }
        
        public void Handle(RecalculateForecastOnSkillCollectionEvent @event)
        {
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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
                unitOfWork.PersistAll();
            }
        }
    }
}