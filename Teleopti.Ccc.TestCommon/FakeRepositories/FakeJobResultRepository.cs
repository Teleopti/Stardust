using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeJobResultRepository : IJobResultRepository
	{
		protected readonly ICollection<IJobResult> _result = new Collection<IJobResult>();

		public void Add(IJobResult entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId(Guid.NewGuid());
			_result.Add(entity);
		}

		public void Remove(IJobResult entity)
		{
			throw new NotImplementedException();
		}

		public IJobResult Get(Guid id)
		{
			return _result.Single(x => x.Id.Value == id);
		}

		public virtual IEnumerable<IJobResult> LoadAll()
		{
			return _result.ToList();
		}

		public IJobResult Load(Guid id)
		{
			throw new NotImplementedException();
		}

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

		public IList<IJobResult> LoadAllWithNoLock()
		{
			return LoadAll().ToList();
		}
	}

	public class FakeJobResultRepositoryForCurrentBusinessUnit : FakeJobResultRepository
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public FakeJobResultRepositoryForCurrentBusinessUnit(ICurrentBusinessUnit currentBusinessUnit)
		{
			_currentBusinessUnit = currentBusinessUnit;
		}

		public override IEnumerable<IJobResult> LoadAll()
		{
			return _result.Where(r => (r as JobResult).BusinessUnit?.Id == _currentBusinessUnit.Current()?.Id).ToList();
		}
	}
}