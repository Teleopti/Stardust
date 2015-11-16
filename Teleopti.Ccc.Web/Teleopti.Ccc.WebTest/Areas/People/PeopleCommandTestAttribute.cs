using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.People.Core.IoC;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	public class PeopleCommandTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new PeopleAreaModule());

			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			system.UseTestDouble<FakeWorkShiftRuleSetRepository>().For<IWorkShiftRuleSetRepository>();
			
        }
	}

	public class FakeWorkShiftRuleSetRepository: IWorkShiftRuleSetRepository
	{
		private readonly IList<IWorkShiftRuleSet> workShiftRuleSets = new List<IWorkShiftRuleSet>();
		public void Add(IWorkShiftRuleSet root)
		{
			workShiftRuleSets.Add(root);
        }

		public void Remove(IWorkShiftRuleSet root)
		{
			throw new NotImplementedException();
		}

		public IWorkShiftRuleSet Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IWorkShiftRuleSet> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IWorkShiftRuleSet Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IWorkShiftRuleSet> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IWorkShiftRuleSet> FindAllWithLimitersAndExtenders()
		{
			throw new NotImplementedException();
		}
	}

	public class FakeRuleSetBagRepository : IRuleSetBagRepository
	{
		private readonly IList<IRuleSetBag> ruleSetBags = new List<IRuleSetBag>();

		public void Add(IRuleSetBag root)
		{
			ruleSetBags.Add(root);
		}

		public void Remove(IRuleSetBag root)
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag Get(Guid id)
		{
			return ruleSetBags.Single(s => s.Id.GetValueOrDefault() == id);
		}

		public IList<IRuleSetBag> LoadAll()
		{
			return ruleSetBags;
		}

		public IRuleSetBag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRuleSetBag> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
		{
			return ruleSetBags;
		}

		public IRuleSetBag Find(Guid id)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeContractScheduleRepository : IContractScheduleRepository
	{
		private readonly IList<IContractSchedule> contractSchedules = new List<IContractSchedule>();

		public void Add(IContractSchedule root)
		{
			contractSchedules.Add(root);
		}

		public void Remove(IContractSchedule root)
		{
			throw new NotImplementedException();
		}

		public IContractSchedule Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IContractSchedule> LoadAll()
		{
			return contractSchedules;
		}

		public IContractSchedule Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IContractSchedule> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IContractSchedule> FindAllContractScheduleByDescription()
		{
			throw new NotImplementedException();
		}

		public ICollection<IContractSchedule> LoadAllAggregate()
		{
			throw new NotImplementedException();
		}
	}

	public class FakePartTimePercentageRepository : IPartTimePercentageRepository
	{
		private IList<IPartTimePercentage> partTimePercentages = new List<IPartTimePercentage>();

		public void Add(IPartTimePercentage root)
		{
			partTimePercentages.Add(root);
		}

		public void Remove(IPartTimePercentage root)
		{
			throw new NotImplementedException();
		}

		public IPartTimePercentage Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPartTimePercentage> LoadAll()
		{
			return partTimePercentages;
		}

		public IPartTimePercentage Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPartTimePercentage> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IPartTimePercentage> FindAllPartTimePercentageByDescription()
		{
			throw new NotImplementedException();
		}
	}

	public class FakeContractRepository : IContractRepository
	{
		private IList<IContract> contracts = new List<IContract>();

		public void Add(IContract root)
		{
			contracts.Add(root);
		}

		public void Remove(IContract root)
		{
			throw new NotImplementedException();
		}

		public IContract Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IContract> LoadAll()
		{
			return contracts;
		}

		public IContract Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IContract> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IContract> FindAllContractByDescription()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IContract> FindContractsContain(string searchString, int itemsLeftToLoad)
		{
			throw new NotImplementedException();
		}
	}
}
