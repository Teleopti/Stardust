using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeContractScheduleRepository : IContractScheduleRepository
	{
		private IList<IContractSchedule> _contractSchedules = new List<IContractSchedule>();

		public void Has(ContractSchedule contractSchedule)
		{
			_contractSchedules.Add(contractSchedule);
		}

		public void Add(IContractSchedule root)
		{
			throw new NotImplementedException();
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
			return _contractSchedules.ToArray();
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
			return _contractSchedules;
		}

	}
}