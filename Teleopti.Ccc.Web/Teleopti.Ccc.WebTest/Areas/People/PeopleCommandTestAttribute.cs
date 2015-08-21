using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
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

			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
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
	}
}
