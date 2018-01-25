﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalPerformanceDataRepository : IExternalPerformanceDataRepository
	{
		private readonly IList<IExternalPerformanceData> _performanceDataList;

		public FakeExternalPerformanceDataRepository()
		{
			_performanceDataList = new List<IExternalPerformanceData>();
		}

		public void Add(IExternalPerformanceData model)
		{
			var existingData = _performanceDataList.FirstOrDefault(x => x.DateFrom == model.DateFrom
																		&& x.PersonId == model.PersonId
																		&& x.ExternalPerformance.ExternalId == model.ExternalPerformance.ExternalId);
			if (existingData != null)
			{
				_performanceDataList.Remove(existingData);
			}
			_performanceDataList.Add(model);
		}

		public void Remove(IExternalPerformanceData model)
		{
			_performanceDataList.Remove(Get(model.Id.Value));
		}

		public IExternalPerformanceData Get(Guid id)
		{
			return _performanceDataList.FirstOrDefault(x => x.Id == id);
		}

		public IExternalPerformanceData Load(Guid id)
		{
			return Get(id);
		}

		public IList<IExternalPerformanceData> LoadAll()
		{
			return _performanceDataList;
		}

		public ICollection<IExternalPerformanceData> FindByPeriod(DateTimePeriod period)
		{
			return _performanceDataList.Where(externalPerformanceData => period.Contains(externalPerformanceData.DateFrom)).ToList();
		}

		public ICollection<IExternalPerformanceData> Find(DateTime date, List<Guid> personIds, int performanceId)
		{
			return _performanceDataList.Where(x =>
				x.DateFrom == date && personIds.Contains(x.PersonId) && x.ExternalPerformance.ExternalId == performanceId).ToList();
		}
	}
}
