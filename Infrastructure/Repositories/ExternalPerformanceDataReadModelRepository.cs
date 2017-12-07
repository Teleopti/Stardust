using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceDataReadModelRepository: IExternalPerformanceDataReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ExternalPerformanceDataReadModelRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void Add(ExternalPerformanceData model)
		{
			var updateList = _currentUnitOfWork.Session().CreateSQLQuery(
				$@"SELECT * FROM ReadModel.ExternalPerformanceData 
					WHERE Person=:{nameof(model.Person)} AND DateFrom=:{nameof(model.DateFrom)}")
				.SetInt32(nameof(model.Score), model.Score)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ExternalPerformanceData)))
				.List<ExternalPerformanceData>();

			if (updateList.Any()) return;

			_currentUnitOfWork.Session().CreateSQLQuery(
					$@"EXEC [ReadModel].[ExternalPerformanceData] 
						@ExternalPerformance=:{nameof(model.ExternalPerformance)},
						@DateFrom=:{nameof(model.DateFrom)},
						@StartDateTime=:{nameof(model.Person)},
						@EndDateTime=:{nameof(model.OriginalPersonId)},
						@Workday=:{nameof(model.Score)}")
				.SetGuid(nameof(model.ExternalPerformance), model.ExternalPerformance)
				.SetDateOnly(nameof(model.DateFrom), model.DateFrom)
				.SetGuid(nameof(model.Person), model.Person)
				.SetString(nameof(model.OriginalPersonId), model.OriginalPersonId)
				.SetInt32(nameof(model.Score), model.Score)
				.ExecuteUpdate();
		}
	}
}
