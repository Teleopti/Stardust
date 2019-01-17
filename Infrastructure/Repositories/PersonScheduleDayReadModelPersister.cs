using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NHibernate;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonScheduleDayReadModelPersister : IPersonScheduleDayReadModelPersister
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IMessageCreator _messageBroker;
		private readonly ICurrentDataSource _currentDataSource;

		private static readonly ILog logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelPersister));

		public PersonScheduleDayReadModelPersister(ICurrentUnitOfWork currentUnitOfWork, IMessageCreator messageBroker, ICurrentDataSource currentDataSource)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_messageBroker = messageBroker;
			_currentDataSource = currentDataSource;
		}

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad)
		{
			logger.Debug("Persisting model PersonScheduleDayReadModel");

			var changed = false;

			if (readModels != null)
			{
				logger.Debug("Saving model PersonScheduleDayReadModel");
				readModels.ForEach(readModel =>
				{
					var count = SaveReadModel(readModel, initialLoad);
					if (count > 0)
					{
						changed = true;
					}
				});
			}

			if (changed)
				_currentUnitOfWork.Current().AfterSuccessfulTx(() =>
				{
					logger.Debug("Sending notification for persisted model IPersonScheduleDayReadModel");
					_messageBroker.Send(_currentDataSource.CurrentName(), businessUnitId, period.StartDate.Date, period.EndDate.Date,
						Guid.Empty, personId, typeof (Person), Guid.Empty, typeof (IPersonScheduleDayReadModel),
						DomainUpdateType.NotApplicable, null);
				});
		}

		public int SaveReadModel(PersonScheduleDayReadModel model, bool initialLoad)
		{
			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Trying to save model PersonScheduleDayReadModel on date {model.BelongsToDate} for person {model.PersonId}, Start {model.Start}, End {model.End}, LoadedScheduleTime {model.ScheduleLoadTimestamp}");
			}

			var updatedCount = _currentUnitOfWork.Session().CreateSQLQuery(
				$@"EXEC ReadModel.UpdatePersonScheduleDay 
					@PersonId=:{nameof(model.PersonId)},
					@Start=:{nameof(model.Start)},
					@End=:{nameof(model.End)},
					@BelongsToDate=:{nameof(model.BelongsToDate)},
					@IsDayOff=:{nameof(model.IsDayOff)},
					@Model=:{nameof(model.Model)},
					@ScheduleLoadedTime=:{nameof(model.ScheduleLoadTimestamp)},
					@IsInitialLoad=:{nameof(initialLoad)},
					@Version=:{nameof(model.Version)}
				")
				.SetGuid(nameof(model.PersonId), model.PersonId)				
				.SetParameter(nameof(model.Start), model.Start)
				.SetParameter(nameof(model.End), model.End)
				.SetDateOnly(nameof(model.BelongsToDate), model.BelongsToDate)
				.SetParameter(nameof(model.IsDayOff), model.IsDayOff)
				.SetParameter(nameof(model.Model), model.Model, NHibernateUtil.Custom(typeof (CompressedString)))
				.SetDateTime(nameof(model.ScheduleLoadTimestamp), model.ScheduleLoadTimestamp)
				.SetBoolean(nameof(initialLoad), initialLoad)
				.SetParameter(nameof(model.Version), model.Version)
				.UniqueResult<int?>() ?? 0;
			return updatedCount;
		}

		public void DeleteReadModel(Guid personId, DateOnly date)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				$"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId = :{nameof(personId)} AND BelongsToDate =:{nameof(date)}")
				.SetGuid(nameof(personId), personId)
				.SetDateOnly(nameof(date), date)
				.ExecuteUpdate();
		}

		public bool IsInitialized()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.PersonScheduleDay")
			                               .List();
			return result.Count > 0;
		}
	}
}