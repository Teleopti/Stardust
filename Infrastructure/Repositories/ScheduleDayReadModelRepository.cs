using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleDayReadModelRepository : IScheduleDayReadModelRepository
	{
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;

	    public ScheduleDayReadModelRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
		    _currentUnitOfWork = currentUnitOfWork;
		}

		public ScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
            return _currentUnitOfWork.Session().CreateSQLQuery(
				$@"SELECT 
						PersonId, 
						BelongsToDate AS Date, 
						StartDateTime, 
						EndDateTime, 
						Workday, 
						Label, 
						DisplayColor AS ColorCode, 
						WorkTime AS WorkTimeTicks, 
						ContractTime AS ContractTimeTicks, 
						NotScheduled 
					FROM ReadModel.ScheduleDay 
					WHERE PersonId=:{nameof(personId)} AND BelongsToDate = :{nameof(date)}")
				.SetGuid(nameof(personId), personId)
				.SetDateOnly(nameof(date), date)
				.SetResultTransformer(Transformers.AliasToBean(typeof (ScheduleDayReadModel)))
				.SetReadOnly(true)
				.UniqueResult<ScheduleDayReadModel>();
		}

		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
            _currentUnitOfWork.Session().CreateSQLQuery(
				$"DELETE FROM ReadModel.ScheduleDay WHERE PersonId=:{nameof(personId)} AND BelongsToDate BETWEEN :{nameof(period.StartDate)} AND :{nameof(period.EndDate)}")
				.SetGuid(nameof(personId), personId)
				.SetDateOnly(nameof(period.StartDate), period.StartDate)
				.SetDateOnly(nameof(period.EndDate), period.EndDate)
				.ExecuteUpdate();
		}

		public void SaveReadModel(ScheduleDayReadModel model)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
					$@"
					EXEC [ReadModel].[UpdateScheduleDay] 
						@PersonId=:{nameof(model.PersonId)},
						@BelongsToDate=:{nameof(model.BelongsToDate)},
						@StartDateTime=:{nameof(model.StartDateTime)},
						@EndDateTime=:{nameof(model.EndDateTime)},
						@Workday=:{nameof(model.Workday)},
						@WorkTime=:{nameof(model.WorkTime)},
						@ContractTime=:{nameof(model.ContractTime)},
						@Label=:{nameof(model.Label)},
						@DisplayColor=:{nameof(model.DisplayColor)},
						@NotScheduled=:{nameof(model.NotScheduled)},
						@Version=:{nameof(model.Version)}")
					.SetGuid(nameof(model.PersonId), model.PersonId)
					.SetDateTime(nameof(model.StartDateTime), model.StartDateTime)
					.SetDateTime(nameof(model.EndDateTime), model.EndDateTime)
					.SetInt64(nameof(model.WorkTime), model.WorkTime.Ticks)
					.SetInt64(nameof(model.ContractTime), model.ContractTime.Ticks)
					.SetBoolean(nameof(model.Workday), model.Workday)
					.SetString(nameof(model.Label), model.Label)
					.SetInt32(nameof(model.DisplayColor), model.DisplayColor.ToArgb())
					.SetDateOnly(nameof(model.BelongsToDate), model.BelongsToDate)
					.SetBoolean(nameof(model.NotScheduled), model.NotScheduled)
					.SetParameter(nameof(model.Version), model.Version)
					.ExecuteUpdate();
		}

		public bool IsInitialized()
		{
            var result = _currentUnitOfWork.Session().CreateSQLQuery("SELECT TOP 1 * FROM ReadModel.ScheduleDay")
				.List();
			return result.Count > 0;
		}
	}
}