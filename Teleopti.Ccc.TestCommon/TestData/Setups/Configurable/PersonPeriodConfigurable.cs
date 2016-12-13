using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonPeriodConfigurable : IUserSetup
	{
		public DateTime StartDate { get; set; }
		public string Contract { get; set; }
		public string PartTimePercentage { get; set; }
		public string ContractSchedule { get; set; }
		public string Team { get; set; }
		public string ShiftBag { get; set; }
		public string Skill { get; set; }
		public string BudgetGroup { get; set; }
		public string WorkflowControlSet { get; set; }
		public string ExternalLogon { get; set; }
		public int ExternalLogonDataSourceId { get; set; }

		public PersonPeriodConfigurable()
		{
			ExternalLogonDataSourceId = -1;
		}

		public virtual void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
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
			var personPeriod = new PersonPeriod(new DateOnly(StartDate),
			                                    personContract,
			                                    team);

			if (!string.IsNullOrEmpty(ShiftBag))
			{
				var bag = new RuleSetBagRepository(uow)
					.LoadAll()
					.Single(x => x.Description.Name == ShiftBag);
				personPeriod.RuleSetBag = bag;
			}

			if (!string.IsNullOrEmpty(Skill))
			{
				var skillRepository = new SkillRepository(uow);
				var skill = skillRepository.LoadAll().Single(c => c.Name == Skill);
				personPeriod.AddPersonSkill(new PersonSkill(skill,new Percent(1.0)));
			}
			
			if (!string.IsNullOrEmpty(BudgetGroup))
			{
				var budgetGroupRepository = new BudgetGroupRepository(uow);
				var budgetGroup = budgetGroupRepository.LoadAll().Single(b => b.Name == BudgetGroup);
				personPeriod.BudgetGroup = budgetGroup;
			}

			if (!string.IsNullOrEmpty(WorkflowControlSet))
			{
				var workflowControlSetRepository = new WorkflowControlSetRepository(uow);
				var workflowControlSet = workflowControlSetRepository.LoadAll().Single(b => b.Name == WorkflowControlSet);
				user.WorkflowControlSet = workflowControlSet;
			}

			if (!string.IsNullOrEmpty(ExternalLogon))
			{
				var externalLogonRepository = new ExternalLogOnRepository(uow);
				var logon = externalLogonRepository.LoadAll().FirstOrDefault(b => b.AcdLogOnName == user.Name.ToString());
				// if it doesnt exist, create it, but then we need the datasourceid
				if (logon == null && ExternalLogonDataSourceId != -1)
				{
					logon = new ExternalLogOn
					{
						AcdLogOnName = ExternalLogon,// is not used?
						DataSourceId = ExternalLogonDataSourceId,
						AcdLogOnOriginalId = ExternalLogon, // this is the user code the rta receives
						AcdLogOnMartId = -1
					};
					externalLogonRepository.Add(logon);
				}
				user.AddExternalLogOn(logon, personPeriod);
			}

			user.AddPersonPeriod(personPeriod);
		}
		
	}
}