﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonScheduleDayReadModelStorage : IPersonScheduleDayReadModelPersister, IPersonScheduleDayReadModelFinder
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IMessageBrokerSender _messageBroker;

		public PersonScheduleDayReadModelStorage(ICurrentUnitOfWork unitOfWork, IMessageBrokerSender messageBroker)
		{
			_unitOfWork = unitOfWork;
			_messageBroker = messageBroker;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
			                  .SetGuid("personid", personId)
			                  .SetDateTime("startdate", startDate)
			                  .SetDateTime("enddate", endDate)
			                  .SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
			                  .SetReadOnly(true)
			                  .List<PersonScheduleDayReadModel>()
				;
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			return ForPerson(date, date, personId).FirstOrDefault();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE TeamId=:TeamId AND ShiftStart IS NOT NULL AND ShiftStart < :DateEnd AND ShiftEnd > :DateStart")
				.SetGuid("TeamId", teamId)
				.SetDateTime("DateStart", period.StartDateTime)
				.SetDateTime("DateEnd", period.EndDateTime)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
				 .SetReadOnly(true)
				 .List<PersonScheduleDayReadModel>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			_unitOfWork.Session().CreateSQLQuery(
				"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
				.SetGuid("person", personId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SaveReadModel(PersonScheduleDayReadModel model)
		{
			_unitOfWork.Session().CreateSQLQuery(
				"INSERT INTO ReadModel.PersonScheduleDay (Id,PersonId,TeamId,SiteId,BusinessUnitId,ShiftStart,ShiftEnd,BelongsToDate,Shift) VALUES (:Id,:PersonId,:TeamId,:SiteId,:BusinessUnitId,:ShiftStart,:ShiftEnd,:BelongsToDate,:Shift)")
				.SetGuid("Id", Guid.NewGuid())
				.SetGuid("PersonId", model.PersonId)
				.SetGuid("TeamId", model.TeamId)
				.SetGuid("SiteId", model.SiteId)
				.SetGuid("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("ShiftStart", model.ShiftStart)
				.SetParameter("ShiftEnd", model.ShiftEnd)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("Shift", model.Shift,NHibernateUtil.StringClob)
				.ExecuteUpdate();

			_unitOfWork.Current().AfterSuccessfulTx(() => _messageBroker.SendEventMessage(null, model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(IPerson), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
		}

		public bool IsInitialized()
		{
			var result = _unitOfWork.Session().CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.PersonScheduleDay")
				.List();
			return result.Count > 0;
		}
	}

}