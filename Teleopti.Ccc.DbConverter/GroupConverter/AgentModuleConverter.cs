using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class AgentModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AgentModuleConverter));

        public AgentModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
        }

        protected override string ModuleName
        {
            get { return "Person"; }
        }

        protected override IEnumerable<Type> DependedOn
        {
            get 
            {
                return new[] { typeof(ForecastModuleConverter), typeof(CommonConverter)};
            }
        }

        protected override void GroupConvert()
        {
            AddContracts();
            AddContractSchedules();
            AddPartTimePercentages();
            ConvertExternalLogOns();
            ConvertAgents();
        }

        private void AddContracts()
        {
            Logger.Info("Creating Default Contracts...");
            ObjectPairCollection<WorktimeType, IContract> contractMapList =
                new ObjectPairCollection<WorktimeType, IContract>();
            using(IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ContractRepository contractRep = new ContractRepository(uow);

                Contract defaultContractFixedNormal = new Contract("Fixed Staff Normal");
                defaultContractFixedNormal.EmploymentType = Teleopti.Interfaces.Domain.EmploymentType.FixedStaffNormalWorkTime;
                defaultContractFixedNormal.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));
                contractMapList.Add((WorktimeType)3, defaultContractFixedNormal);
                contractRep.Add(defaultContractFixedNormal);

				//Contract defaultContractFixedPeriod = new Contract("Fixed Staff Period");
				//defaultContractFixedPeriod.EmploymentType = Teleopti.Interfaces.Domain.EmploymentType.FixedStaffPeriodWorkTime;
				//defaultContractFixedPeriod.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));
				//contractMapList.Add((WorktimeType)1, defaultContractFixedPeriod);
				//contractRep.Add(defaultContractFixedPeriod);

                Contract defaultContractFixedDay = new Contract("Fixed Staff Day");
                defaultContractFixedDay.EmploymentType = Teleopti.Interfaces.Domain.EmploymentType.FixedStaffDayWorkTime;
                defaultContractFixedDay.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));
                contractMapList.Add((WorktimeType)2, defaultContractFixedDay);
                contractRep.Add(defaultContractFixedDay);

                Contract defaultContractHourly = new Contract("Hourly Staff");
                defaultContractHourly.EmploymentType = Teleopti.Interfaces.Domain.EmploymentType.HourlyStaff;
                defaultContractHourly.WorkTime = new WorkTime(new TimeSpan(0, 0, 0));
                contractMapList.Add((WorktimeType)4, defaultContractHourly);
                contractMapList.Add((WorktimeType)5, defaultContractHourly);
                contractRep.Add(defaultContractHourly);

                uow.PersistAll();
            }
            MappedObjectPair.Contract = contractMapList;
        }

        private void AddContractSchedules()
        {
            
            Logger.Info("Creating Default ContractSchedule...");
            ObjectPairCollection<WorktimeType, IContractSchedule> contractScheduleMapList =
                new ObjectPairCollection<WorktimeType, IContractSchedule>();
            using(IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())  
            {
                ContractScheduleRepository contractScheduleRep = new ContractScheduleRepository(uow);

                ContractSchedule defaultContractScheduleFixed = new ContractSchedule("Fixed Staff");
                contractScheduleMapList.Add((WorktimeType)1, defaultContractScheduleFixed);
                contractScheduleMapList.Add((WorktimeType)2, defaultContractScheduleFixed);
                contractScheduleMapList.Add((WorktimeType)3, defaultContractScheduleFixed);
                contractScheduleRep.Add(defaultContractScheduleFixed);

                ContractSchedule defaultContractScheduleHourly = new ContractSchedule("Hourly Staff");
                contractScheduleMapList.Add((WorktimeType)4, defaultContractScheduleHourly);
                contractScheduleMapList.Add((WorktimeType)5, defaultContractScheduleHourly);
                contractScheduleRep.Add(defaultContractScheduleHourly);

                uow.PersistAll();
            }
            MappedObjectPair.ContractSchedule = contractScheduleMapList;
        }

        private void AddPartTimePercentages()
        {
            Logger.Info("Creating Default PartTimePercentages...");
            ObjectPairCollection<WorktimeType, IPartTimePercentage> partTimePercentageMapList =
                new ObjectPairCollection<WorktimeType, IPartTimePercentage>();
            using(IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())  
            {
                PartTimePercentageRepository partTimePercentageRep = new PartTimePercentageRepository(uow);

                PartTimePercentage defaultPartTimePercentage = new PartTimePercentage("100%");
                partTimePercentageMapList.Add((WorktimeType)1, defaultPartTimePercentage);
                partTimePercentageMapList.Add((WorktimeType)2, defaultPartTimePercentage);
                partTimePercentageMapList.Add((WorktimeType)3, defaultPartTimePercentage);
                partTimePercentageMapList.Add((WorktimeType)4, defaultPartTimePercentage);
                partTimePercentageMapList.Add((WorktimeType)5, defaultPartTimePercentage);
                partTimePercentageRep.Add(defaultPartTimePercentage);

                uow.PersistAll();
            }
            MappedObjectPair.PartTimePercentage = partTimePercentageMapList;
        }

        private void ConvertAgents()
        {
            Logger.Info("Converting Agents...");


            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                //Todo : this line need to be remove when mapping of PersonPeriod and ersoncontract finalized
                PersonContract dummyPersonContract=CreateDummyPersonContract(uow);

                AgentMapper agentMapper = new AgentMapper(MappedObjectPair, TimeZoneInfo,dummyPersonContract);
                AgentConverter agentConverter = new AgentConverter(uow, agentMapper);
                agentConverter.ConvertAndPersist(new AgentReader().GetAll().Values);
            }
        }

        private void ConvertExternalLogOns()
        {
            Logger.Info("Converting ACD Logins...");
            
            //Really ugly way to get all current Logins...
            ICollection<Agent> agents = new AgentReader().GetAll().Values;

            IEnumerable<int> intList = (from a in agents
                                        from p in a.PeriodCollection
                                        where p.LoginId.HasValue && p.LoginId.Value > -1
                                        select p.LoginId.Value).Distinct();


            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ExternalLogOnConverter loginConverter = new ExternalLogOnConverter(uow, new ExternalLogOnMapper(MappedObjectPair, TimeZoneInfo));
                loginConverter.ConvertAndPersist(intList);
            }
        }


        private PersonContract CreateDummyPersonContract(IUnitOfWork uow)
        {            
            ContractRepository contractRep = new ContractRepository(uow);

            Contract defaultContractFixed = new Contract("Fixed Staff");
            defaultContractFixed.EmploymentType = Interfaces.Domain.EmploymentType.FixedStaffNormalWorkTime;
            defaultContractFixed.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));
            foreach (var definitionSet in MappedObjectPair.MultiplicatorDefinitionSet.Obj2Collection())
            {
                defaultContractFixed.AddMultiplicatorDefinitionSetCollection(definitionSet);
            }
            contractRep.Add(defaultContractFixed);

            PartTimePercentageRepository partTimePercentageRep = new PartTimePercentageRepository(uow);

            PartTimePercentage partTimePercentage = new PartTimePercentage("Dummy Part Time Percentge");
            partTimePercentageRep.Add(partTimePercentage);

            ContractScheduleRepository contractScheduleRep = new ContractScheduleRepository (uow);

            ContractSchedule contractSchedule = new ContractSchedule("Dummy Contract Schedule");
            contractScheduleRep.Add(contractSchedule);
            
            uow.PersistAll();

            PersonContract personContract=new PersonContract(defaultContractFixed,partTimePercentage,contractSchedule);     
  
            return personContract;
        }
            
    }
}