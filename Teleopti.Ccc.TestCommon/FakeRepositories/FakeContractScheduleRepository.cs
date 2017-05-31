using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeContractScheduleRepository : IContractScheduleRepository
	{
		private readonly IList<IContractSchedule> _contractSchedules = new List<IContractSchedule>();

		public void Has(ContractSchedule contractSchedule)
		{
			_contractSchedules.Add(contractSchedule);
		}

		public void Add(IContractSchedule root)
		{
			_contractSchedules.Add(root);
		}

		public void Remove(IContractSchedule root)
		{
			throw new NotImplementedException();
		}

		public IContractSchedule Get(Guid id)
		{
			return _contractSchedules.FirstOrDefault(c => id == c.Id);
		}

		public IList<IContractSchedule> LoadAll()
		{
			return _contractSchedules.ToArray();
		}

		public IContractSchedule Load(Guid id)
		{
			throw new NotImplementedException();
		}

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