using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

		public IEnumerable<PersonScheduleDayReadModel> ForPersonsByFilteredTimes(DateOnly shiftTradeDate, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filter)
		{
			var idlist = string.Join(",", personIdList);
			var filterStartTimeStarts = string.Join(",", filter.StartTimeStarts.Select(d => d.ToString("yyyy-MM-dd HH:mm")));
			var filterStartTimeEnds = string.Join(",", filter.StartTimeEnds.Select(d => d.ToString("yyyy-MM-dd HH:mm")));
			var filterEndTimeStarts = string.Join(",", filter.EndTimeStarts.Select(d => d.ToString("yyyy-MM-dd HH:mm")));
			var filterEndTimeEnds = string.Join(",", filter.EndTimeEnds.Select(d => d.ToString("yyyy-MM-dd HH:mm")));
			return _unitOfWork.Session().CreateSQLQuery(
				@"EXEC  [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter] @shiftTradeDate=:shiftTradeDate, @personList=:personIdList, 
							@filterStartTimeStarts=:filterStartTimeStarts, @filterStartTimeEnds=:filterStartTimeEnds, 
							@filterEndTimeStarts=:filterEndTimeStarts, @filterEndTimeEnds=:filterEndTimeEnds, 
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
							  .SetParameter("filterStartTimeStarts", filterStartTimeStarts, NHibernateUtil.StringClob)
							  .SetParameter("filterStartTimeEnds", filterStartTimeEnds, NHibernateUtil.StringClob)
							  .SetParameter("filterEndTimeStarts", filterEndTimeStarts, NHibernateUtil.StringClob)
							  .SetParameter("filterEndTimeEnds", filterEndTimeEnds, NHibernateUtil.StringClob)
							  .SetParameter("skip", paging.Skip)
							  .SetParameter("take", paging.Take)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}
	}


}