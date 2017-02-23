using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeJobResultRepository : IJobResultRepository
	{
		private readonly ICollection<IJobResult> _result = new Collection<IJobResult>();

		public void Add(IJobResult entity)
		{
			entity.SetId(Guid.NewGuid());
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
			return _result.ToList();
		}

		public IJobResult Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<IJobResult> LoadHistoryWithPaging(PagingDetail pagingDetail, params string[] jobCategories)
		{
			return _result.Take(5).ToList();
		}

		public void AddDetailAndCheckSuccess(Guid jobResultId, IJobResultDetail detail, int expectedSuccessful)
		{
			var jobResult = Get(jobResultId);
			jobResult.AddDetail(detail);
			if (jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null) >= expectedSuccessful)
				jobResult.FinishedOk = true;
		}

		public IJobResult FindWithNoLock(Guid jobResultId)
		{
			return Get(jobResultId);
		}
	}
}