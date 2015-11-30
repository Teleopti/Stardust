using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakeJobResultRepository : IJobResultRepository
	{
		private ICollection<IJobResult> _result;
		public FakeJobResultRepository()
		{
			_result = new Collection<IJobResult>();
		}

		public void Add(IJobResult entity)
		{
			_result.Add(entity);
		}

		public void Remove(IJobResult entity)
		{
			throw new NotImplementedException();
		}

		public IJobResult Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IJobResult> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IJobResult Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IJobResult> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<IJobResult> LoadHistoryWithPaging(PagingDetail pagingDetail, params string[] jobCategories)
		{
			return _result.Take(5).ToList();
		}
	}
}