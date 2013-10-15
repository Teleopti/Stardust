using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
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

		public PersonPeriodConfigurable() {
			Contract = GlobalDataMaker.Data().Data<CommonContract>().Contract.Description.Name;
			PartTimePercentage = GlobalDataMaker.Data().Data<CommonPartTimePercentage>().PartTimePercentage.Description.Name;
			ContractSchedule = GlobalDataMaker.Data().Data<CommonContractSchedule>().ContractSchedule.Description.Name;
			Team = GlobalDataMaker.Data().Data<CommonTeam>().Team.Description.Name;
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
				personPeriod.AddPersonSkill(new PersonSkill(skill,new Percent(1.0)){Active = true});
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

			user.AddPersonPeriod(personPeriod);
		}
	}
}