using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Domain;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class ScheduleModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleModuleConverter));
        private readonly string _connectionString;

        public ScheduleModuleConverter(MappedObjectPair mappedObjectPair, 
            DateTimePeriod period,
            TimeZoneInfo timeZoneInfo, string connectionString)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
               
            _connectionString = connectionString;
        }

        protected override string ModuleName
        {
            get { return "Schedule"; }
        }

        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new[] { typeof(CommonConverter), typeof(AgentModuleConverter) };
            }
        }

        protected override void GroupConvert()
        {
            
            foreach (Scenario scenario in new ScenarioReader().GetAll().Values)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                ConvertAgentAssignments(scenario);
                stopwatch.Stop();
                Logger.InfoFormat("Converting agent days in scenario {0} took {1}", scenario.Name, stopwatch.Elapsed);
            }

            ConvertRotations();
            ConvertPersonRotations();
            ConvertingAvailability();
            ConvertPersonAvailability();
            ConvertPreferences();
        }
        private void ConvertPreferences()
        {
					DateTime currentDateTime = Period.LocalStartDateTime(TimeZoneInfo);
            const int daysToAdd = 14;
						while (currentDateTime < Period.LocalEndDateTime(TimeZoneInfo))
            {
                DatePeriod currentPeriod = new DatePeriod(currentDateTime, calcEndTime(currentDateTime, daysToAdd));
                Logger.InfoFormat("Converting agent preferences {0} ", currentPeriod);
                IList<global::Domain.AgentDayPreference> preferences = new PreferenceReader().LoadPreferences(currentPeriod);

                Logger.InfoFormat("Loaded {0} old agent days...", preferences.Count);

                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    var agentPreferenceConverter = new AgentPreferenceConverter(uow, new PreferenceMapper(MappedObjectPair, TimeZoneInfo));
                    agentPreferenceConverter.ConvertAndPersist(preferences);
                }
                currentDateTime = currentDateTime.AddDays(daysToAdd);
            }
        }


        private void ConvertAgentAssignments(Scenario scenario)
        {
            IList<global::Domain.Agent> agentList = new List<global::Domain.Agent>();

            DateTime currentDateTime = Period.LocalStartDateTime(TimeZoneInfo);
            const int daysToAdd = 14;
						while (currentDateTime < Period.LocalEndDateTime(TimeZoneInfo))
            {
                var currentPeriod = new DatePeriod(currentDateTime, calcEndTime(currentDateTime,daysToAdd));
                Logger.InfoFormat("Converting agent days in scenario {0} {1}", scenario.Name, currentPeriod);
                IAgentDayCollection agDays = new AgentDayReader(true, scenario).LoadAgentDays(currentPeriod);

                IList<AgentDay> list2 = EmptyAgentDays(agDays);

                foreach (AgentDay agentDay in list2)
                {
                    agDays.Remove(agentDay);
                }

                AddAgentsToListForAbsenceConvert(agentList, agDays);

                Logger.InfoFormat("Loaded {0} old agent days...", agDays.ItemCollection.Count);

                    foreach (DateTime dateTime in currentPeriod.DateCollection())
                    {
                        using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                        {
                            IList<AgentDay> list = agDays.get_ItemCollection(dateTime.Date);

                            var assConverter = new AgentAssignmentConverter(uow, new AgentAssignmentMapper(MappedObjectPair, TimeZoneInfo));
                            assConverter.ConvertAndPersist(list);
                            uow.Clear();

                            var absActConverter = new AgentAbsenceActivityConverter(uow, new AgentAbsenceActivityMapper(MappedObjectPair, TimeZoneInfo));
                            absActConverter.ConvertAndPersist(list);
                            uow.Clear();

                            var agDayOffConverter = new AgentDayOffConverter(uow, new AgentDayOffMapper(MappedObjectPair, TimeZoneInfo));
                            agDayOffConverter.ConvertAndPersist(list);

                            var noteConverter = new NoteConverter(uow, new NoteMapper(MappedObjectPair, TimeZoneInfo));
                            noteConverter.ConvertAndPersist(list);

                            if (scenario.DefaultScenario)
                            {
                                var studentAvailabilityConverter = new StudentAvailabilityConverter(uow, new StudentAvailabilityMapper(MappedObjectPair));
                                studentAvailabilityConverter.ConvertAndPersist(list);
                            }
                        }
                    }
  
                currentDateTime = currentDateTime.AddDays(daysToAdd);
            }

            ConvertAgentAbsences(agentList, scenario);
        }

        private static void AddAgentsToListForAbsenceConvert(IList<global::Domain.Agent> agentList, IAgentDayCollection list)
        {
            foreach (global::Domain.AgentDay agentDay in list)
            {
                if(!agentList.Contains(agentDay.AssignedAgent))
                    agentList.Add(agentDay.AssignedAgent);

            }
        }

        private void ConvertAgentAbsences(IList<global::Domain.Agent> agentList, Scenario scenario)
        {
					var period = new DatePeriod(Period.LocalStartDateTime(TimeZoneInfo), Period.LocalEndDateTime(TimeZoneInfo));
            IDictionary<global::Domain.Agent, IList<global::Domain.AgentDay>> agentDayDic = new Dictionary<global::Domain.Agent, IList<global::Domain.AgentDay>>();

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                foreach (global::Domain.Agent agent in agentList)
                {
                    IAgentDayCollection agDays = new AgentDayReader(true, scenario).LoadAgentDays(period, agent);
                    IList<AgentDay> list = agDays.get_ItemCollection(agent);

                    agentDayDic.Add(agent, list);

                    var absConverter = new AgentAbsenceConverter(uow, new AgentAbsenceMapper(MappedObjectPair, TimeZoneInfo));
                    absConverter.ConvertAndPersist(agentDayDic);
                    agentDayDic.Clear();
                    uow.Clear();

                }
            }
        }

        private DateTime calcEndTime(DateTime currentDateTime, int daysToAdd)
        {
            if (currentDateTime.AddDays(daysToAdd - 1) > Period.EndDateTime)
                return Period.EndDateTime;

            return currentDateTime.AddDays(daysToAdd - 1);
        }

        private static IList<AgentDay> EmptyAgentDays(IEnumerable<AgentDay> agDays)
        {
            return agDays.Where(ad => !(ad.AgentDayAssignment.AssignmentType != AssignedType.NotAssigned ||
                               (ad.AgentDayAssignment.AssignmentType == AssignedType.NotAssigned &&
                                ad.Limitation != null &&
                                ad.Limitation.CoreTime == null) ||
                                (ad.AgentDayAssignment.AssignmentType == AssignedType.NotAssigned &&
                                ad.Limitation != null &&
                                ad.Limitation.WebCoreTime != null))).ToList();
        }

        private void ConvertRotations()
        {
            Logger.Info("Converting Rotations...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IDayOffTemplate> dayOffs = new DayOffRepository(uow).FindAllDayOffsSortByDescription();

                var helper = new RotationConvertHelper(_connectionString);
                var rotationsConverter = new RotationsConverter(uow, new RotationsMapper(MappedObjectPair, helper.LoadAllRotationDays(), new SystemSettingReader().GetSystemSetting.IntervalLength(), dayOffs));
                rotationsConverter.ConvertAndPersist(helper.LoadAllRotations());
            }
        }

        private void ConvertPersonRotations()
        {
            Logger.Info("Converting Person Rotations...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var helper = new RotationConvertHelper(_connectionString);
                var rotationsConverter = new PersonRotationConverter(uow, new PersonRotationMapper(MappedObjectPair));
                rotationsConverter.ConvertAndPersist(helper.LoadAllEmployeeRotations());
            }
        }
        private void ConvertingAvailability()
        {
            Logger.Info("Converting Availability...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var helper = new AvailabilityConverterHelper(_connectionString);
                var availabilityConverter = new AvailabilityConverter(uow, new AvailabilityMapper(MappedObjectPair, helper.LoadAllAvailabilityDays()));
                availabilityConverter.ConvertAndPersist(helper.LoadAllAvailability());
            }
        }
        private void ConvertPersonAvailability()
        {
            Logger.Info("Converting Person Availability...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var helper = new AvailabilityConverterHelper(_connectionString);
                var availabilityConverter = new PersonAvailabilityConverter(uow, new PersonAvailabilityMapper(MappedObjectPair));
                availabilityConverter.ConvertAndPersist(helper.LoadAllEmployeeAvailability());
            }
        }
    }
}
