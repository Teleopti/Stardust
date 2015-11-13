using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
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
			_unitOfWork = new ThisUnitOfWork(unitOfWork);
		}

		public PersonScheduleDayReadModelFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			const string sql
				= "SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], IsDayOff, Model "
				  + "FROM ReadModel.PersonScheduleDay "
				  + "WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate";
			return _unitOfWork.Session().CreateSQLQuery(sql)
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("Start", NHibernateUtil.DateTime)
				.AddScalar("End", NHibernateUtil.DateTime)
				.AddScalar("IsDayOff", NHibernateUtil.Boolean)
				.AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
				.SetDateOnly("startdate", startDate)
				.SetDateOnly("enddate", endDate)
				.SetGuid("personid", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
				.SetReadOnly(true)
				.List<PersonScheduleDayReadModel>();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			return ForPerson(date, date, personId).FirstOrDefault();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			const string sql
				= "SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], Model "
				  + "FROM ReadModel.PersonScheduleDay "
				  + "WHERE PersonId In (:PersonIds) AND Start IS NOT NULL AND Start < :DateEnd AND [End] > :DateStart";
			var res = new List<PersonScheduleDayReadModel>();
			foreach (var ids in personIds.Batch(2000))
			{
				res.AddRange(_unitOfWork.Session().CreateSQLQuery(sql)
					.AddScalar("PersonId", NHibernateUtil.Guid)
					.AddScalar("TeamId", NHibernateUtil.Guid)
					.AddScalar("SiteId", NHibernateUtil.Guid)
					.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
					.AddScalar("Date", NHibernateUtil.DateTime)
					.AddScalar("Start", NHibernateUtil.DateTime)
					.AddScalar("End", NHibernateUtil.DateTime)
					.AddScalar("Model", NHibernateUtil.Custom(typeof (CompressedString)))
					.SetParameterList("PersonIds", ids)
					.SetDateTime("DateStart", period.StartDateTime)
					.SetDateTime("DateEnd", period.EndDateTime)
					.SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
					.SetReadOnly(true)
					.List<PersonScheduleDayReadModel>());
			}
			return res;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging)
		{
			const string sql
				= @"EXEC [ReadModel].[LoadShiftTradeBulletinSchedules]"
				+" @shiftExchangeOfferIdList =:shiftExchangeOfferIdList,"
				+ "@skip=:skip, "
				+ "@take=:take";
			var idlist = string.Join(",", shiftExchangeOfferIdList);
			return _unitOfWork.Session().CreateSQLQuery(sql)
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("FirstName", NHibernateUtil.String)
				.AddScalar("LastName", NHibernateUtil.String)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("Start", NHibernateUtil.DateTime)
				.AddScalar("End", NHibernateUtil.DateTime)
				.AddScalar("Model", NHibernateUtil.Custom(typeof (CompressedString)))
				.AddScalar("ShiftExchangeOffer", NHibernateUtil.Guid)
				.AddScalar("MinStart", NHibernateUtil.DateTime)
				.AddScalar("Total", NHibernateUtil.Int16)
				.AddScalar("IsLastPage", NHibernateUtil.Boolean)
				.SetParameter("shiftExchangeOfferIdList", idlist, NHibernateUtil.StringClob)
				.SetParameter("skip", paging.Skip)
				.SetParameter("take", paging.Take)
				.SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
				.SetReadOnly(true)
				.List<PersonScheduleDayReadModel>();
		}
	
		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly shiftTradeDate,
			IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filter = null, string timeSortOrder ="" )
		{
			const string sql
				= @"EXEC  [ReadModel].[LoadPersonSchedule] "
				  + "@scheduleDate=:shiftTradeDate, "
				  + "@personList=:personIdList, "
				  + "@filterStartTimes=:filterStartTimes, "
				  + "@filterEndTimes=:filterEndTimes, "
				  + "@isDayOff=:isDayOff,"
				  + "@isEmptyDay=:isEmptyDay,"
				  + "@isWorkingDay=:isWorkingDay,"
				  + "@skip=:skip, @take=:take,"
				  + "@timeSortOrder=:timeSortOrder";
			var idlist = string.Join(",", personIdList);
			var filterString = getTimeFilterString(filter);
			return _unitOfWork.Session().CreateSQLQuery(sql)
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("FirstName", NHibernateUtil.String)
				.AddScalar("LastName", NHibernateUtil.String)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("Start", NHibernateUtil.DateTime)
				.AddScalar("End", NHibernateUtil.DateTime)
				.AddScalar("Model", NHibernateUtil.Custom(typeof (CompressedString)))
				.AddScalar("MinStart", NHibernateUtil.DateTime)
				.AddScalar("Total", NHibernateUtil.Int16)
				.AddScalar("IsLastPage", NHibernateUtil.Boolean)
				.SetDateOnly("shiftTradeDate", shiftTradeDate)
				.SetParameter("personIdList", idlist, NHibernateUtil.StringClob)
				.SetParameter("filterStartTimes", filterString.startTimes, NHibernateUtil.StringClob)
				.SetParameter("filterEndTimes", filterString.endTimes, NHibernateUtil.StringClob)
				.SetParameter("isDayOff", filter == null || filter.IsDayOff, NHibernateUtil.Boolean)
				.SetParameter("isEmptyDay", filter == null || filter.IsEmptyDay, NHibernateUtil.Boolean)
				.SetParameter("isWorkingDay", filter == null || filter.IsWorkingDay, NHibernateUtil.Boolean)
				.SetParameter("skip", paging.Skip)
				.SetParameter("take", paging.Take)
				.SetString("timeSortOrder", timeSortOrder)
				.SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
				.SetReadOnly(true)
				.List<PersonScheduleDayReadModel>();
		}	

		private class TimeFilterString
		{
			public string startTimes;
			public string endTimes;
		}

		private TimeFilterString getTimeFilterString(TimeFilterInfo filter)
		{
			const string dateTimeFormat = "yyyy-MM-dd HH:mm";
			var filterString = new TimeFilterString() { startTimes = "", endTimes = ""};

			if (filter == null) return filterString;

			var startTimes = filter.StartTimes ?? new List<DateTimePeriod>();
			var startTimesAsString
				= from s in startTimes
					let start = s.StartDateTime.ToString(dateTimeFormat)
					let end = s.EndDateTime.ToString(dateTimeFormat)
					select new
					{
						startTime = start + ";" + end,
					};
			var endTimes = filter.EndTimes ?? new List<DateTimePeriod>();
			var endTimesAsString
				= from e in endTimes
					let start = e.StartDateTime.ToString(dateTimeFormat)
					let end = e.EndDateTime.ToString(dateTimeFormat)
					select new
					{
						endTime = start + ";" + end,
					};
			filterString.startTimes = string.Join(",", startTimesAsString.Select(d => d.startTime));
			filterString.endTimes = string.Join(",", endTimesAsString.Select(d => d.endTime));

			return filterString;
		}
	}
}