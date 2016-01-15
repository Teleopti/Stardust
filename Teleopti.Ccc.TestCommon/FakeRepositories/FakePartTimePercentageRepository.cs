using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePartTimePercentageRepository : IPartTimePercentageRepository
	{
		public void Has(PartTimePercentage partTimePercentage)
		{
		}

		public void Add(IPartTimePercentage root)
		{
			throw new NotImplementedException();
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
			return new List<IPartTimePercentage>();
		}

		public IPartTimePercentage Load(Guid id)
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
}