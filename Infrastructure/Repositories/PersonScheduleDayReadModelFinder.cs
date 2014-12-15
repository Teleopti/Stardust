using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
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
			_unitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}

		public PersonScheduleDayReadModelFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], IsDayOff, Model FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
			                  .AddScalar("PersonId", NHibernateUtil.Guid)
			                  .AddScalar("TeamId", NHibernateUtil.Guid)
			                  .AddScalar("SiteId", NHibernateUtil.Guid)
			                  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
			                  .AddScalar("Date", NHibernateUtil.DateTime)
			                  .AddScalar("Start", NHibernateUtil.DateTime)
			                  .AddScalar("End", NHibernateUtil.DateTime)
									.AddScalar("IsDayOff", NHibernateUtil.Boolean)
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
			var res = new List<PersonScheduleDayReadModel>();
			foreach (var ids in personIds.Batch(2000))
			{
				res.AddRange(_unitOfWork.Session().CreateSQLQuery(
					"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, Start, [End], Model FROM ReadModel.PersonScheduleDay WHERE PersonId In (:PersonIds) AND Start IS NOT NULL AND Start < :DateEnd AND [End] > :DateStart")
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

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, DateTimePeriod mySchedulePeriod, Paging paging)
		{
			var idlist = string.Join(",", personIdList);
			return _unitOfWork.Session().CreateSQLQuery(
				@"EXEC  [ReadModel].[LoadShiftTradeBulletinSchedules] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList,
										@currentScheduleStart=:myScheduleStart, @currentScheduleEnd=:myScheduleEnd, @skip=:skip, @take=:take")
							  .AddScalar("PersonId", NHibernateUtil.Guid)
							  .AddScalar("TeamId", NHibernateUtil.Guid)
							  .AddScalar("SiteId", NHibernateUtil.Guid)
							  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
							  .AddScalar("Date", NHibernateUtil.DateTime)
							  .AddScalar("Start", NHibernateUtil.DateTime)
							  .AddScalar("End", NHibernateUtil.DateTime)
							  .AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
							  .AddScalar("ShiftExchangeOffer", NHibernateUtil.Guid)
							  .AddScalar("MinStart", NHibernateUtil.DateTime)
							  .AddScalar("Total", NHibernateUtil.Int16)
							  .AddScalar("IsLastPage", NHibernateUtil.Boolean)
							  .SetDateTime("shiftTradeDate", shiftTradeDate)
							  .SetParameter("personIdList", idlist, NHibernateUtil.StringClob)
							  .SetParameter("myScheduleStart", mySchedulePeriod.StartDateTime)
							  .SetParameter("myScheduleEnd", mySchedulePeriod.EndDateTime)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersonsWithTimeFilter(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, DateTimePeriod mySchedulePeriod, Paging paging, TimeFilterInfo filter)
		{
			var idlist = string.Join(",", personIdList);
			var filterString = getTimeFilterString(filter);
			return _unitOfWork.Session().CreateSQLQuery(
				@"EXEC  [ReadModel].[LoadShiftTradeBulletinSchedulesWithTimeFilter] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList,
										@currentScheduleStart=:myScheduleStart, @currentScheduleEnd=:myScheduleEnd, @filterStartTimes=:filterStartTimes, @filterEndTimes=:filterEndTimes,
										@isDayOff=:isDayOff, @skip=:skip, @take=:take")
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
							  .SetParameter("personIdList", idlist, NHibernateUtil.StringClob)
							  .SetParameter("myScheduleStart", mySchedulePeriod.StartDateTime)
							  .SetParameter("myScheduleEnd", mySchedulePeriod.EndDateTime)
							  .SetParameter("filterStartTimes", filterString.startTimes, NHibernateUtil.StringClob)
							  .SetParameter("filterEndTimes", filterString.endTimes, NHibernateUtil.StringClob)
							  .SetParameter("isDayOff", filter.IsDayOff, NHibernateUtil.Boolean)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}


		public IEnumerable<PersonScheduleDayReadModel> ForPersonsIncludeEmptyDays(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, Paging paging)
		{
			var idlist = string.Join(",", personIdList);
			return _unitOfWork.Session().CreateSQLQuery(
				"EXEC  [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList, @skip=:skip, @take=:take")
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

		public IEnumerable<PersonScheduleDayReadModel> ForPersonsByFilteredTimes(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filter)
		{
			var idlist = string.Join(",", personIdList);
			var filterString = getTimeFilterString(filter);
			return _unitOfWork.Session().CreateSQLQuery(
				@"EXEC  [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList, 
							@filterStartTimes=:filterStartTimes, @filterEndTimes=:filterEndTimes, @isDayOff=:isDayOff,
							@skip=:skip, @take=:take")
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
							  .SetParameter("filterStartTimes", filterString.startTimes, NHibernateUtil.StringClob)
							  .SetParameter("filterEndTimes", filterString.endTimes, NHibernateUtil.StringClob)
							  .SetParameter("isDayOff", filter.IsDayOff, NHibernateUtil.Boolean)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}

		class TimeFilterString
		{
			public string startTimes;
			public string endTimes;
		}

		private TimeFilterString getTimeFilterString(TimeFilterInfo filter)
		{
			var filterString = new TimeFilterString();
			var startTimesAsString = from s in filter.StartTimes
											 let start = s.StartDateTime.ToString("yyyy-MM-dd HH:mm")
											 let end = s.EndDateTime.ToString("yyyy-MM-dd HH:mm")
											 select new
											 {
												 startTime = start + ";" + end,
											 };
			var endTimesAsString = from e in filter.EndTimes
										  let start = e.StartDateTime.ToString("yyyy-MM-dd HH:mm")
										  let end = e.EndDateTime.ToString("yyyy-MM-dd HH:mm")
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