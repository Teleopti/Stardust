using System;
using System.Collections.Generic;
using log4net;
using NHibernate;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonScheduleDayReadModelPersister : IPersonScheduleDayReadModelPersister
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IMessageCreator _messageBroker;
		private readonly ICurrentDataSource _currentDataSource;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelPersister));

		public PersonScheduleDayReadModelPersister(ICurrentUnitOfWork currentUnitOfWork, IMessageCreator messageBroker, ICurrentDataSource currentDataSource)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_messageBroker = messageBroker;
			_currentDataSource = currentDataSource;
		}

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad)
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Persisting model PersonScheduleDayReadModel");

			if (readModels != null)
			{
				if (Logger.IsDebugEnabled)
					Logger.Debug("Saving model PersonScheduleDayReadModel");
				readModels.ForEach(readModel => saveReadModel(readModel, initialLoad));
			}

			if (!initialLoad)
				_currentUnitOfWork.Current().AfterSuccessfulTx(() =>
				{
					if (Logger.IsDebugEnabled)
						Logger.Debug("Sending notification for persisted model IPersonScheduleDayReadModel");
					_messageBroker.Send(_currentDataSource.CurrentName(), businessUnitId, period.StartDate.Date, period.EndDate.Date,
						Guid.Empty, personId, typeof (Person), Guid.Empty, typeof (IPersonScheduleDayReadModel),
						DomainUpdateType.NotApplicable, null);
				});
		}

		private void saveReadModel(PersonScheduleDayReadModel model, bool initialLoad)
		{
			if (Logger.IsDebugEnabled)
				Logger.DebugFormat(
					"Trying to save model PersonScheduleDayReadModel on date {0} for person {1}, Start {2}, End {3}, LoadedScheduleTime {4}",
					model.BelongsToDate, model.PersonId, model.Start, model.End, model.ScheduleLoadTimestamp);

			_currentUnitOfWork.Session().CreateSQLQuery(
				"EXEC ReadModel.UpdatePersonScheduleDay @PersonId=:PersonId,@TeamId=:TeamId,@SiteId=:SiteId,@BusinessUnitId=:BusinessUnitId,@Start=:Start,@End=:End,@BelongsToDate=:BelongsToDate,@IsDayOff=:IsDayOff,@Model=:Model,@ScheduleLoadedTime=:ScheduleLoadedTime,@IsInitialLoad=:IsInitialLoad")
				.SetGuid("PersonId", model.PersonId)
				.SetGuid("TeamId", model.TeamId)
				.SetGuid("SiteId", model.SiteId)
				.SetGuid("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("Start", model.Start)
				.SetParameter("End", model.End)
				.SetDateOnly("BelongsToDate", model.BelongsToDate)
				.SetParameter("IsDayOff", model.IsDayOff)
				.SetParameter("Model", model.Model, NHibernateUtil.Custom(typeof (CompressedString)))
				.SetDateTime("ScheduleLoadedTime", model.ScheduleLoadTimestamp)
				.SetBoolean("IsInitialLoad", initialLoad)
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