using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Envers;
using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleHistoryRepository : IScheduleHistoryRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ScheduleHistoryRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IEnumerable<IRevision> FindRevisions(IPerson agent, DateOnly dateOnly, int maxResult)
		{
			InParameter.ValueMustBeLargerThanZero("maxResult", maxResult);

			var revisionIds = new HashSet<long>();
			var dateTime = convertFromDateOnly(agent, dateOnly);
			findRevisionsForAssignment(agent, dateTime, maxResult).ForEach(revId => revisionIds.Add(revId));
			findRevisionsForAbsence(agent, dateTime, maxResult).ForEach(revId => revisionIds.Add(revId));
			findRevisionsForDayOff(agent, dateTime, maxResult).ForEach(revId => revisionIds.Add(revId));
			return loadRevisions(revisionIds, maxResult);
		}

		public IEnumerable<IPersistableScheduleData> FindSchedules(IRevision revision, IPerson agent, DateOnly dateOnly)
		{
			var ret = new List<IPersistableScheduleData>();
			var dateTime = convertFromDateOnly(agent, dateOnly);
			findAssignment(agent, dateTime, revision).ForEach(ret.Add);
			findAbsence(agent, dateTime, revision).ForEach(ret.Add);
			findDayOff(agent, dateTime, revision).ForEach(ret.Add);
			return ret;
		}

		private IEnumerable<PersonDayOff> findDayOff(IPerson agent, DateTimePeriod dateTimePeriod, IRevision revision)
		{
			return session().Auditer().CreateQuery()
				.ForEntitiesAtRevision<PersonDayOff>(revision.Id)
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("DayOff.Anchor").Between(dateTimePeriod.StartDateTime, dateTimePeriod.EndDateTime))
				.Results();
		}

		private IEnumerable<PersonAbsence> findAbsence(IPerson agent, DateTimePeriod dateTimePeriod, IRevision revision)
		{
			return session().Auditer().CreateQuery()
				.ForEntitiesAtRevision<PersonAbsence>(revision.Id)
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Le(dateTimePeriod.EndDateTime))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Ge(dateTimePeriod.StartDateTime))
				.Results();
		}

		private IEnumerable<PersonAssignment> findAssignment(IPerson agent, DateTimePeriod dateTimePeriod, IRevision revision)
		{
			return session().Auditer().CreateQuery()
				.ForEntitiesAtRevision<PersonAssignment>(revision.Id)
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Period.period.Minimum").Le(dateTimePeriod.EndDateTime))
				.Add(AuditEntity.Property("Period.period.Maximum").Ge(dateTimePeriod.StartDateTime))
				.Results();
		}

		private IEnumerable<long> findRevisionsForAssignment(IPerson agent, DateTimePeriod dateTimePeriod, int maxSize)
		{
			return session().Auditer().CreateQuery()
				.ForRevisionsOfEntity(typeof (PersonAssignment), false, true)
				.AddProjection(AuditEntity.RevisionNumber())
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Period.period.Minimum").Le(dateTimePeriod.EndDateTime))
				.Add(AuditEntity.Property("Period.period.Maximum").Ge(dateTimePeriod.StartDateTime))
				.AddOrder(AuditEntity.RevisionNumber().Desc())
				.SetMaxResults(maxSize)
				.GetResultList<long>();
		}

		private IEnumerable<long> findRevisionsForAbsence(IPerson agent, DateTimePeriod dateTimePeriod, int maxSize)
		{
			return session().Auditer().CreateQuery()
				.ForRevisionsOfEntity(typeof (PersonAbsence), false, true)
				.AddProjection(AuditEntity.RevisionNumber())
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Le(dateTimePeriod.EndDateTime))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Ge(dateTimePeriod.StartDateTime))
				.AddOrder(AuditEntity.RevisionNumber().Desc())
				.SetMaxResults(maxSize)
				.GetResultList<long>();
		}

		private IEnumerable<long> findRevisionsForDayOff(IPerson agent, DateTimePeriod dateTimePeriod, int maxSize)
		{
			return session().Auditer().CreateQuery()
				.ForRevisionsOfEntity(typeof (PersonDayOff), false, true)
				.AddProjection(AuditEntity.RevisionNumber())
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("DayOff.Anchor").Between(dateTimePeriod.StartDateTime, dateTimePeriod.EndDateTime))
				.AddOrder(AuditEntity.RevisionNumber().Desc())
				.SetMaxResults(maxSize)
				.GetResultList<long>();
		}


		private IEnumerable<IRevision> loadRevisions(IEnumerable<long> ids, int maxSize)
		{
			//flytta till revisionrepository? G�r det vid n�sta s�nt h�r historyrepository...
			var cutIds = (from id in ids orderby id descending 
								select id).Take(maxSize).ToArray();

			return session().CreateCriteria<Revision>()
				.Add(Restrictions.In("Id", cutIds))
				.AddOrder(Order.Desc("Id"))
				.List<IRevision>();
		}

		private static DateTimePeriod convertFromDateOnly(IPerson agent, DateOnly date)
		{
			return new DateOnlyPeriod(date, date).ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone());
		}

		private ISession session()
		{
			return ((NHibernateUnitOfWork) _unitOfWorkFactory.CurrentUnitOfWork()).Session;
		}
	}
}
