using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
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
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Model FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
			                  .AddScalar("PersonId", NHibernateUtil.Guid)
			                  .AddScalar("TeamId", NHibernateUtil.Guid)
			                  .AddScalar("SiteId", NHibernateUtil.Guid)
			                  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
			                  .AddScalar("Date", NHibernateUtil.DateTime)
			                  .AddScalar("ShiftStart", NHibernateUtil.DateTime)
			                  .AddScalar("ShiftEnd", NHibernateUtil.DateTime)
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

		public IEnumerable<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Model FROM ReadModel.PersonScheduleDay WHERE TeamId=:TeamId AND ShiftStart IS NOT NULL AND ShiftStart < :DateEnd AND ShiftEnd > :DateStart")
			                  .AddScalar("PersonId", NHibernateUtil.Guid)
			                  .AddScalar("TeamId", NHibernateUtil.Guid)
			                  .AddScalar("SiteId", NHibernateUtil.Guid)
			                  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
			                  .AddScalar("Date", NHibernateUtil.DateTime)
			                  .AddScalar("ShiftStart", NHibernateUtil.DateTime)
			                  .AddScalar("ShiftEnd", NHibernateUtil.DateTime)
			                  .AddScalar("Model", NHibernateUtil.Custom(typeof (CompressedString)))
			                  .SetGuid("TeamId", teamId)
			                  .SetDateTime("DateStart", period.StartDateTime)
			                  .SetDateTime("DateEnd", period.EndDateTime)
			                  .SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
			                  .SetReadOnly(true)
			                  .List<PersonScheduleDayReadModel>();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Model FROM ReadModel.PersonScheduleDay WHERE PersonId IN (:personIdList) AND BelongsToDate=:date ORDER BY ShiftStart")
							  .AddScalar("PersonId", NHibernateUtil.Guid)
							  .AddScalar("TeamId", NHibernateUtil.Guid)
							  .AddScalar("SiteId", NHibernateUtil.Guid)
							  .AddScalar("BusinessUnitId", NHibernateUtil.Guid)
							  .AddScalar("Date", NHibernateUtil.DateTime)
							  .AddScalar("ShiftStart", NHibernateUtil.DateTime)
							  .AddScalar("ShiftEnd", NHibernateUtil.DateTime)
							  .AddScalar("Model", NHibernateUtil.Custom(typeof(CompressedString)))
							  .SetDateTime("date", date)
							  .SetParameterList("personIdList", personIdList)
							  .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
							  .SetReadOnly(true)
							  .List<PersonScheduleDayReadModel>();
		}
	}
}