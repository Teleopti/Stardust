﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePartTimePercentageRepository : IPartTimePercentageRepository
	{
		private readonly IList<IPartTimePercentage> _partTimePercentages = new List<IPartTimePercentage>(); 

		public void Has(PartTimePercentage partTimePercentage)
		{
			Add(partTimePercentage);
		}

		public void Add(IPartTimePercentage root)
		{
			_partTimePercentages.Add(root);
		}

		public void Remove(IPartTimePercentage root)
		{
			throw new NotImplementedException();
		}

		public IPartTimePercentage Get(Guid id)
		{
			return _partTimePercentages.FirstOrDefault(p => id == p.Id);
		}

		public IList<IPartTimePercentage> LoadAll()
		{
			return _partTimePercentages.ToArray();
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