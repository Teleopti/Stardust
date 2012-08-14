using System;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Team = Teleopti.Ccc.WebBehaviorTest.Data.User.Team;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class BackgroundStepDefinitions
	{
		[Given(@"I am a user")]
		public void GivenIAmAUser()
		{
		}

		[Given(@"I have the role '(.*)'")]
		public void GivenIHaveTheRoleAccessToMytime(string name)
		{
			var userRole = new UserRole {Name = name};
			UserFactory.User().Setup(userRole);
		}

		[Given(@"I have the workflow control set '(.*)'")]
		public void GivenIHaveTheWorkflowControlSetPublishedSchedule(string name)
		{
			var userWorkflowControlSet = new UserWorkflowControlSet { Name = name };
			UserFactory.User().Setup(userWorkflowControlSet);
		}



		[Given(@"I have a schedule period with")]
		public void GivenIHaveASchedulePeriodWith(Table table)
		{
			var schedulePeriod = table.CreateInstance<SchedulePeriodFromTable>();
			UserFactory.User().Setup(schedulePeriod);
		}

		[Given(@"I have a person period with")]
		public void GivenIHaveAPersonPeriodWith(Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodFromTable>();
			UserFactory.User().Setup(personPeriod);
		}

		[Given(@"I have a preference with")]
		public void GivenIHaveAPreferenceWith(Table table)
		{
			var preference = table.CreateInstance<PreferenceFromTable>();
			UserFactory.User().Setup(preference);
		}

		[Given(@"I have an extended preference on '(.*)'")]
		public void GivenIHaveAnExtendedPreferenceOn2012_06_20(DateTime date)
		{
			UserFactory.User().Setup(new PreferenceFromTable { Date = date, IsExtended = true });
		}

		[When(@"I view preferences for date '(.*)'")]
		public void WhenIViewPreferencesForDate2012_06_20(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoPreference(date);
		}





		[Given(@"there is a workflow control set with")]
		public void GivenThereIsAWorkflowControlSetWith(Table table)
		{
			var workflowControlSet = table.CreateInstance<WorkflowControlSetFromTable>();
			UserFactory.User().Setup(workflowControlSet);
		}

		[Given(@"there is a role with")]
		public void GivenThereIsARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleFromTable>();
			UserFactory.User().Setup(role);
		}

		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnit = table.CreateInstance<BusinessUnitFromTable>();
			UserFactory.User().Setup(businessUnit);
		}

	}

	public class PreferenceFromTable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public bool IsExtended { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var restriction = new PreferenceRestriction();

			if (IsExtended)
				restriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));
			
			var preferenceDay = new PreferenceDay(user, new DateOnly(Date), restriction);

			var preferenceDayRepository = new PreferenceDayRepository(uow);
			preferenceDayRepository.Add(preferenceDay);
		}
	}


	public class PersonPeriodFromTable : IUserSetup
	{
		public DateTime StartDate { get; set; }
		public string Contract { get; set; }
		public string PartTimePercentage { get; set; }
		public string ContractSchedule { get; set; }
		public string Team { get; set; }

		public PersonPeriodFromTable() {
			Contract = GlobalDataContext.Data().Data<CommonContract>().Contract.Description.Name;
			PartTimePercentage = GlobalDataContext.Data().Data<CommonPartTimePercentage>().PartTimePercentage.Description.Name;
			ContractSchedule = GlobalDataContext.Data().Data<CommonContractSchedule>().ContractSchedule.Description.Name;
			Team = GlobalDataContext.Data().Data<CommonTeam>().Team.Description.Name;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var contractRepository = new ContractRepository(uow);
			var contract = contractRepository.LoadAll().Single(c => c.Description.Name == Contract);

			var partTimePercentageRepository = new PartTimePercentageRepository(uow);
			var partTimePercentage = partTimePercentageRepository.LoadAll().Single(c => c.Description.Name == PartTimePercentage);

			var contractScheduleRepository = new ContractScheduleRepository(uow);
			var contractSchedule = contractScheduleRepository.LoadAll().Single(c => c.Description.Name == ContractSchedule);

			var teamRepository = new TeamRepository(uow);
			var team = teamRepository.LoadAll().Single(c => c.Description.Name == Team);

			var personContract = new PersonContract(contract,
													partTimePercentage,
													contractSchedule);
			var personPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(StartDate),
																 personContract,
			                                                     team);
			user.AddPersonPeriod(personPeriod);
		}
	}

	public class SchedulePeriodFromTable : IUserSetup
	{
		public DateTime StartDate { get; set; }
		public SchedulePeriodType Type { get; set; }
		public int Length { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var schedulePeriod = new Domain.Scheduling.Assignment.SchedulePeriod(
				new DateOnly(StartDate),
				Type,
				Length);
			user.AddSchedulePeriod(schedulePeriod);
		}

	}

	public class UserRole : IUserSetup, IUserRoleSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var roleRepository = new ApplicationRoleRepository(uow);
			var role = roleRepository.LoadAll().Single(b => b.Name == Name);

			user.PermissionInformation.AddApplicationRole(role);
		}
	}

	public class UserWorkflowControlSet : IUserSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var repository = new WorkflowControlSetRepository(uow);
			var workflowControlSet = repository.LoadAll().Single(w => w.Name == Name);

			user.WorkflowControlSet = workflowControlSet;
		}
	}

	public class BusinessUnitFromTable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnitFactory.CreateSimpleBusinessUnit(Name));
		}
	}

	public class RoleFromTable : IDataSetup
	{
		public string Name { get; set; }
		public string BusinessUnit { get; set; }
		public bool ViewUnpublishedSchedules { get; set; }
		public bool ViewConfidential { get; set; }
		public bool AccessToMobileReports { get; set; }

		public RoleFromTable()
		{
			BusinessUnit = GlobalDataContext.Data().Data<CommonBusinessUnit>().BusinessUnit.Description.Name;
			ViewUnpublishedSchedules = false;
			ViewConfidential = false;
			AccessToMobileReports = true;
		}

		public void Apply(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var allApplicationFunctions = applicationFunctionRepository.GetAllApplicationFunctionSortedByCode().AsEnumerable();

			var applicationFunctions = from f in allApplicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.All select f;

			if (!ViewUnpublishedSchedules)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules select f;

			if (!ViewConfidential)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential select f;

			if (!AccessToMobileReports)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere select f;

			var role = ApplicationRoleFactory.CreateRole(Name, null);

			var availableData = new AvailableData
			                    	{
			                    		ApplicationRole = role,
			                    		AvailableDataRange = AvailableDataRangeOption.MyTeam
			                    	};

			role.AvailableData = availableData;

			var businessUnitRepository = new BusinessUnitRepository(uow);
			var businessUnit = businessUnitRepository.LoadAllBusinessUnitSortedByName().Single(b => b.Name == BusinessUnit);
			role.SetBusinessUnit(businessUnit);
			applicationFunctions.ToList().ForEach(role.AddApplicationFunction);

			var applicationRoleRepository = new ApplicationRoleRepository(uow);
			var availableDataRepository = new AvailableDataRepository(uow);

			applicationRoleRepository.Add(role);
			availableDataRepository.Add(availableData);

		}
	}

	public class WorkflowControlSetFromTable : IDataSetup
	{
		public string Name { get; set; }
		public string SchedulePublishedToDate { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var workflowControlSet = new WorkflowControlSet(Name) {SchedulePublishedToDate = DateTime.Parse(SchedulePublishedToDate)};
			var repository = new WorkflowControlSetRepository(uow);
			repository.Add(workflowControlSet);
		}
	}
}