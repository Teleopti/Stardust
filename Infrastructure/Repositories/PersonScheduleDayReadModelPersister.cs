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

			if (!initialLoad)
				clearPeriodForPerson(period, personId);

			if (readModels != null)
			{
				if (Logger.IsDebugEnabled)
					Logger.Debug("Saving model PersonScheduleDayReadModel");
				readModels.ForEach(saveReadModel);
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

		private void clearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
			                  .SetGuid("person", personId)
							  .SetDateOnly("StartDate", period.StartDate)
							  .SetDateOnly("EndDate", period.EndDate)
			                  .ExecuteUpdate();
		}

		private void saveReadModel(PersonScheduleDayReadModel model)
		{
			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("Saving model PersonScheduleDayReadModel on date {0} for person {1}, Start {2}, End {3}", model.BelongsToDate, model.PersonId, model.Start, model.End);

			_currentUnitOfWork.Session().CreateSQLQuery(
				"INSERT INTO ReadModel.PersonScheduleDay (PersonId,TeamId,SiteId,BusinessUnitId,Start,[End],BelongsToDate,IsDayOff,Model) VALUES (:PersonId,:TeamId,:SiteId,:BusinessUnitId,:Start,:End,:BelongsToDate,:IsDayOff,:Model)")
			                  .SetGuid("PersonId", model.PersonId)
			                  .SetGuid("TeamId", model.TeamId)
			                  .SetGuid("SiteId", model.SiteId)
			                  .SetGuid("BusinessUnitId", model.BusinessUnitId)
			                  .SetParameter("Start", model.Start)
			                  .SetParameter("End", model.End)
							  .SetDateOnly("BelongsToDate", model.BelongsToDate)
									.SetParameter("IsDayOff", model.IsDayOff)
			                  .SetParameter("Model", model.Model, NHibernateUtil.Custom(typeof(CompressedString)))
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