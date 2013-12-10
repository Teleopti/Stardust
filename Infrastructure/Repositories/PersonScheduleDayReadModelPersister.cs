﻿using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonScheduleDayReadModelPersister : IPersonScheduleDayReadModelPersister
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IMessageBrokerSender _messageBroker;
		private readonly ICurrentDataSource _currentDataSource;

		public PersonScheduleDayReadModelPersister(ICurrentUnitOfWork currentUnitOfWork, IMessageBrokerSender messageBroker, ICurrentDataSource currentDataSource)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_messageBroker = messageBroker;
			_currentDataSource = currentDataSource;
		}

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad)
		{
			if (!initialLoad)
				clearPeriodForPerson(period, personId);

			if (readModels != null)
				readModels.ForEach(saveReadModel);

			if (!initialLoad)
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _messageBroker.SendEventMessage(_currentDataSource.CurrentName(), businessUnitId, period.StartDate, period.EndDate, Guid.Empty, personId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
		}

		private void clearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
			                  .SetGuid("person", personId)
			                  .SetDateTime("StartDate", period.StartDate)
			                  .SetDateTime("EndDate", period.EndDate)
			                  .ExecuteUpdate();
		}

		private void saveReadModel(PersonScheduleDayReadModel model)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				"INSERT INTO ReadModel.PersonScheduleDay (PersonId,TeamId,SiteId,BusinessUnitId,Start,[End],BelongsToDate,Model) VALUES (:Id,:PersonId,:TeamId,:SiteId,:BusinessUnitId,:Start,:End,:BelongsToDate,:Model)")
			                  .SetGuid("PersonId", model.PersonId)
			                  .SetGuid("TeamId", model.TeamId)
			                  .SetGuid("SiteId", model.SiteId)
			                  .SetGuid("BusinessUnitId", model.BusinessUnitId)
			                  .SetParameter("Start", model.Start)
			                  .SetParameter("End", model.End)
			                  .SetDateTime("BelongsToDate", model.BelongsToDate)
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