using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public class SmartPartModel : ISmartPartModel
    {
        private IList<EntityUpdateInformation> _workloadUpdatedInfo;
        private IList<NamedEntity> _workloadNames;
        private IScenario _defaultScenario;
        private IList<IScenario> _scenarios = new List<IScenario>();
        private ISkill _skill;
        private Guid _skillId;

        public IScenario DefaultScenario
        {
            get
            {
                if (_defaultScenario == null)
                {
                    using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        ScenarioRepository scenarioRepository = new ScenarioRepository(uow);
                        _defaultScenario = scenarioRepository.LoadDefaultScenario();
                    }
                    return _defaultScenario;
                }
                return _defaultScenario;
            }
        }

        public ISkill Skill
        {
            get
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    SkillRepository skillRep = new SkillRepository(uow);
                    _skill = skillRep.Get(_skillId);

                    LazyLoadingManager.Initialize(_skill.UpdatedBy);
                    var workloads = _skill.WorkloadCollection.OrderBy(s => s.Name);
                    foreach (var workload in workloads)
                    {
                        LazyLoadingManager.Initialize(workload);
                        LazyLoadingManager.Initialize(workload.UpdatedBy);
                    }
                    return _skill;
                }
            }
        }

        public Guid SkillId
        {
            get { return _skillId; }
            set { _skillId = value; }
        }

        public IList<NamedEntity> WorkloadNames
        {
            get { return _workloadNames; }
        }

        public IList<EntityUpdateInformation> WorkloadUpdatedInfo
        {
            get { return _workloadUpdatedInfo; }
        }

        public IList<IScenario> Scenarios
        {
            get
            {
                if (_scenarios.Count == 0)
                {
                    using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        ScenarioRepository scenarioRepository = new ScenarioRepository(uow);
                        _scenarios = scenarioRepository.LoadAll().ToList();
                    }
                    return _scenarios;
                }
                return _scenarios;
            }
        }

        public IScenario GetScenarioById(Guid scenarioId)
        {
            foreach (var scenario in Scenarios)
            {
                if (scenario.Id.Equals(scenarioId))
                    return scenario;
            }
            return null;
        }

        public IDictionary<Guid, IList<IForecastProcessReport>> ProcessValidations(DateOnlyPeriod period)
        {
            IDictionary<Guid, IList<IForecastProcessReport>> workDays = new Dictionary<Guid, IList<IForecastProcessReport>>();
            if (_skill != null)
            {
                SetWorkloadsForSmartpart();
                var workloads = _skill.WorkloadCollection.OrderBy(s => s.Name);
                foreach (IWorkload workLoad in workloads)
                {
                    IList<IForecastProcessReport> forecasterReports =
                        ForecastProcessReportRepository.ValidationReport(workLoad, period);
                    workDays.Add(workLoad.Id.GetValueOrDefault(Guid.Empty), forecasterReports);
                }
            }
            return workDays;
        }

        public IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> ProcessAllBudgetForecasting(DateOnlyPeriod period)
        {
            IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> allBudgets = new Dictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>>();
            if (_skill != null)
            {
                SetWorkloadsForSmartpart();
                var workloads = _skill.WorkloadCollection.OrderBy(s => s.Name);
                foreach (var scenario in Scenarios)
                {
                    IDictionary<Guid, IList<IForecastProcessReport>> budgets = new Dictionary<Guid, IList<IForecastProcessReport>>();
                    foreach (IWorkload workLoad in workloads)
                    {
                        IList<IForecastProcessReport> forecasterReports =
                            ForecastProcessReportRepository.BudgetReport(scenario, workLoad, period);
                        budgets.Add(workLoad.Id.GetValueOrDefault(Guid.Empty), forecasterReports);
                    }
                    allBudgets.Add(scenario.Id.GetValueOrDefault(), budgets);
                }
            }
            return allBudgets;
        }

        public IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> ProcessAllDetailedForecasting(DateOnlyPeriod period)
        {
            IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> allDetailed = new Dictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>>();
            if (_skill != null)
            {
                SetWorkloadsForSmartpart();
                var workloads = _skill.WorkloadCollection.OrderBy(s => s.Name);
                foreach (var scenario in Scenarios)
                {
                    IDictionary<Guid, IList<IForecastProcessReport>> detailed = new Dictionary<Guid, IList<IForecastProcessReport>>();
                    foreach (IWorkload workLoad in workloads)
                    {
                        IList<IForecastProcessReport> forecasterReports =
                            ForecastProcessReportRepository.DetailReport(scenario, workLoad, period);
                        detailed.Add(workLoad.Id.GetValueOrDefault(Guid.Empty), forecasterReports);
                    }
                    allDetailed.Add(scenario.Id.GetValueOrDefault(), detailed);
                }
            }
            return allDetailed;
        }

        public void ProcessTemplates()
        {
            SetWorkloadsForSmartpart();
        }

        public IDictionary<IScenario, EntityUpdateInformation> SetLastUpdatedWorkloadDetailedValuesOfAllScenarios(bool longterm)
        {
            IDictionary<IScenario, EntityUpdateInformation> allvalues = new Dictionary<IScenario, EntityUpdateInformation>();

            if (_skill != null)
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    SkillDayRepository skillRepository = new SkillDayRepository(uow);
                    foreach (var scenario in Scenarios)
                    {
                        ISkillDay skillDay = skillRepository.FindLatestUpdated(_skill, scenario, longterm);
                        if (skillDay != null)
                        {
                            EntityUpdateInformation values = new EntityUpdateInformation();
                            TimeZoneInfo timeZoneInfo =
                                TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
                            values.LastUpdate = TimeZoneHelper.ConvertFromUtc(skillDay.UpdatedOn.Value, timeZoneInfo);
	                        values.Name = skillDay.UpdatedBy.Name;
                            values.Tag = scenario.Description.Name;
                            allvalues.Add(scenario, values);
                        }
                    }
                }
            }
            return allvalues;
        }

        public IList<IScenario> FindLastUpdatedScenarios(IDictionary<IScenario, EntityUpdateInformation> allLastUpdatedScenarios)
        {
            if (allLastUpdatedScenarios == null) throw new ArgumentNullException("allLastUpdatedScenarios");

            IList<IScenario> scenarios = new List<IScenario>();
            var updatedScenarios = allLastUpdatedScenarios.OrderBy(s => s.Key.Description.Name);
            string temporaryTicket = string.Format(CultureInfo.CurrentCulture, "{0}{1}",updatedScenarios.GetHashCode(), scenarios.GetHashCode());
            IScenario defaultScenario = new Scenario(temporaryTicket);
            foreach (var scenarioInfo in updatedScenarios)
            {
                if (scenarioInfo.Key.Description.Name.Equals(_defaultScenario.Description.Name))
                    defaultScenario = scenarioInfo.Key;
                else
                    scenarios.Add(scenarioInfo.Key);
            }
            if (!defaultScenario.Description.Name.Equals(temporaryTicket))
                scenarios.Insert(0, defaultScenario);
            return scenarios;
        }

        public EntityUpdateInformation SetLastUpdatedValidatedVolumnDayValues()
        {
            EntityUpdateInformation values = new EntityUpdateInformation();
            if (_skill != null)
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IValidatedVolumeDayRepository validatedVolumeDayRepository = new ValidatedVolumeDayRepository(new ThisUnitOfWork(uow));
                    IValidatedVolumeDay validatedVolumeDay = validatedVolumeDayRepository.FindLatestUpdated(_skill);
                    if (validatedVolumeDay != null)
                    {
                        TimeZoneInfo timeZoneInfo =
                            TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
                        values.LastUpdate = TimeZoneHelper.ConvertFromUtc(validatedVolumeDay.UpdatedOn.Value,
                                                                          timeZoneInfo);
                        values.Name = validatedVolumeDay.UpdatedBy.Name;
                    }
                }
            }
            return values;
        }

        private void SetWorkloadsForSmartpart()
        {
            if ((_skill != null) && (_skill.WorkloadCollection != null))
            {
                _workloadUpdatedInfo = new List<EntityUpdateInformation>();
                _workloadNames = new List<NamedEntity>();
                var workloads = _skill.WorkloadCollection.OrderBy(s => s.Name);
                TimeZoneInfo timeZoneInfo = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;

                foreach (IWorkload workload in workloads)
                {
                    var entityUpdateInformation = new EntityUpdateInformation();
                    entityUpdateInformation.LastUpdate = TimeZoneHelper.ConvertFromUtc(workload.UpdatedOn.Value, timeZoneInfo);
                    if (workload.UpdatedBy != null)
                    {
                        entityUpdateInformation.Name = workload.UpdatedBy.Name;
                        _workloadUpdatedInfo.Add(entityUpdateInformation);
                    }
                    var workloadName = new NamedEntity { Name = workload.Name, Id = workload.Id.GetValueOrDefault(Guid.Empty) };
                    _workloadNames.Add(workloadName);
                }
            }
        }
    }
}