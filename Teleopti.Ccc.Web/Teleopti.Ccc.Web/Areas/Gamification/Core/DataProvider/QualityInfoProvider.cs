using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class QualityInfoProvider : IQualityInfoProvider
	{
		private readonly IStatisticRepository _repository;

		public QualityInfoProvider(IStatisticRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<QualityInfoViewModel> GetAllQualityInfo()
		{
			var qualityInfoList = _repository.LoadAllQualityInfo();
			return qualityInfoList.Select(x => new QualityInfoViewModel
			{
				QualityId = x.QualityId,
				QualityName = x.QualityName,
				QualityType = x.QualityType,
				ScoreWeight = x.ScoreWeight
			});
		}
	}
}