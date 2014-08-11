﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonScheduleDayReadModelFinder : IPersonScheduleDayReadModelFinder
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonScheduleDayReadModelFinder(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull("unitOfWork", unitOfWork);
			_unitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}

		public PersonScheduleDayReadModelFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], Model FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
			                  .AddScalar("PersonId", NHibernateUtil.Guid)
			                  .AddScalar("TeamId", NHibernateUtil.Guid)
			                  .AddScalar("SiteId", NHibernateUtil.Guid)
			                  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
			                  .AddScalar("Date", NHibernateUtil.DateTime)
			                  .AddScalar("Start", NHibernateUtil.DateTime)
			                  .AddScalar("End", NHibernateUtil.DateTime)
			                  .AddScalar("Model", NHibernateUtil.Custom(typeof (CompressedString)))
			                  .SetDateTime("startdate", startDate)
			                  .SetDateTime("enddate", endDate)
							  .SetGuid("personid", personId)
			                  .SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
			                  .SetReadOnly(true)
			                  .List<PersonScheduleDayReadModel>();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			return ForPerson(date, date, personId).FirstOrDefault();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], Model FROM ReadModel.PersonScheduleDay WHERE PersonId In (:PersonIds) AND Start IS NOT NULL AND Start < :DateEnd AND [End] > :DateStart")
							  .AddScalar("PersonId", NHibernateUtil.Guid)
							  .AddScalar("TeamId", NHibernateUtil.Guid)
							  .AddScalar("SiteId", NHibernateUtil.Guid)
							  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
							  .AddScalar("Date", NHibernateUtil.DateTime)
							  .AddScalar("Start", NHibernateUtil.DateTime)
							  .AddScalar("End", NHibernateUtil.DateTime)
							  .AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
							  .SetParameterList("PersonIds", personIds)
							  .SetDateTime("DateStart", period.StartDateTime)
							  .SetDateTime("DateEnd", period.EndDateTime)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, Paging paging)
		{
			var idlist = string.Join(",", personIdList);
			return _unitOfWork.Session().CreateSQLQuery(
				"EXEC  [ReadModel].[LoadPossibleShiftTradeSchedules] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList, @skip=:skip, @take=:take")
							  .AddScalar("PersonId", NHibernateUtil.Guid)
							  .AddScalar("TeamId", NHibernateUtil.Guid)
							  .AddScalar("SiteId", NHibernateUtil.Guid)
							  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
							  .AddScalar("Date", NHibernateUtil.DateTime)
							  .AddScalar("Start", NHibernateUtil.DateTime)
							  .AddScalar("End", NHibernateUtil.DateTime)
							  .AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
							  .AddScalar("MinStart", NHibernateUtil.DateTime)
							  .AddScalar("Total", NHibernateUtil.Int16)
							  .AddScalar("IsLastPage", NHibernateUtil.Boolean)
							  .SetDateTime("shiftTradeDate", shiftTradeDate)
							  .SetParameter("personIdList", idlist,NHibernateUtil.StringClob)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersonsByFilteredTimes(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, Paging paging,
																								IEnumerable<TimePeriod> filteredStartTimes, IEnumerable<TimePeriod> filteredEndTimes)
		{
			var idlist = string.Join(",", personIdList);
			var startTimeList = string.Join(",", filteredStartTimes);
			var endTimeList = string.Join(",", filteredEndTimes);
			return _unitOfWork.Session().CreateSQLQuery(
				"EXEC  [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList, @filteredStartTimeList=:filteredStartTimes, @filteredEndTimeList=:filteredEndTimes, @skip=:skip, @take=:take")
							  .AddScalar("PersonId", NHibernateUtil.Guid)
							  .AddScalar("TeamId", NHibernateUtil.Guid)
							  .AddScalar("SiteId", NHibernateUtil.Guid)
							  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
							  .AddScalar("Date", NHibernateUtil.DateTime)
							  .AddScalar("Start", NHibernateUtil.DateTime)
							  .AddScalar("End", NHibernateUtil.DateTime)
							  .AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
							  .AddScalar("MinStart", NHibernateUtil.DateTime)
							  .AddScalar("Total", NHibernateUtil.Int16)
							  .AddScalar("IsLastPage", NHibernateUtil.Boolean)
							  .SetDateTime("shiftTradeDate", shiftTradeDate)
							  .SetParameter("personIdList", idlist,NHibernateUtil.StringClob)
							  .SetParameter("filteredStartTimes", startTimeList, NHibernateUtil.StringClob)
							  .SetParameter("filteredEndTimes", endTimeList, NHibernateUtil.StringClob)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}
	}
}