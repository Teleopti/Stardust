using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Envers;
using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleHistoryRepository : IScheduleHistoryRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWorkFactory;

		public ScheduleHistoryRepository(ICurrentUnitOfWork unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IEnumerable<IRevision> FindRevisions(IPerson agent, DateOnly dateOnly, int maxResult)
		{
			InParameter.ValueMustBeLargerThanZero(nameof(maxResult), maxResult);

			var revisionIds = new HashSet<long>();
			var dateTime = convertFromDateOnly(agent, dateOnly);
			findRevisionsForAssignment(agent, dateOnly, maxResult).ForEach(revId => revisionIds.Add(revId));
			findRevisionsForAbsence(agent, dateTime, maxResult).ForEach(revId => revisionIds.Add(revId));
			return loadRevisions(revisionIds, maxResult);
		}

		public IEnumerable<IPersistableScheduleData> FindSchedules(IRevision revision, IPerson agent, DateOnly dateOnly)
		{
			var ret = new List<IPersistableScheduleData>();
			var dateTime = convertFromDateOnly(agent, dateOnly);
			ret.AddRange(findAssignment(agent, dateOnly, revision));
			ret.AddRange(findAbsence(agent, dateTime, revision, ret));
			return ret;
		}

		private IEnumerable<PersonAbsence> findAbsence(IPerson agent, DateTimePeriod dateTimePeriod, IRevision revision, IEnumerable<IPersistableScheduleData> assignments)
		{
			var periodEnd = dateTimePeriod.EndDateTime;
			var ass = assignments.FirstOrDefault();
			if (ass != null)
			{
				var assPeriod = ass.Period;
				if (assPeriod.EndDateTime > periodEnd)
				{
					periodEnd = assPeriod.EndDateTime;
				}
			}

			var periodWithExtraAtEnd = new DateTimePeriod(dateTimePeriod.StartDateTime, periodEnd);
			var absences =  session().Auditer().CreateQuery()
				.ForEntitiesAtRevision<PersonAbsence>(revision.Id)
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Lt(periodWithExtraAtEnd.EndDateTime))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Gt(periodWithExtraAtEnd.StartDateTime))
				.Results();

	
			return absences;

		}

		private IEnumerable<PersonAssignment> findAssignment(IPerson agent, DateOnly dateOnly, IRevision revision)
		{
			var assignments = session().Auditer().CreateQuery()
				.ForEntitiesAtRevision<PersonAssignment>(revision.Id)
				.Add(AuditEntity.Property("Person").Eq(agent))
                .Add(AuditEntity.Property("Date").Eq(dateOnly))
				.Results().ToList();

		    foreach (var assignment in assignments)
		    {
                if (!LazyLoadingManager.IsInitialized(assignment.ShiftCategory))
                    LazyLoadingManager.Initialize(assignment.ShiftCategory);
		    }
            return assignments;
		}

		private IEnumerable<long> findRevisionsForAssignment(IPerson agent, DateOnly dateOnly, int maxSize)
		{
			return session().Auditer().CreateQuery()
				.ForRevisionsOfEntity(typeof (PersonAssignment), false, true)
				.AddProjection(AuditEntity.RevisionNumber().Distinct())
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Date").Eq(dateOnly))
				.AddOrder(AuditEntity.RevisionNumber().Desc())
				.SetMaxResults(maxSize)
				.GetResultList<long>();
		}

		private IEnumerable<long> findRevisionsForAbsence(IPerson agent, DateTimePeriod dateTimePeriod, int maxSize)
		{
			return session().Auditer().CreateQuery()
				.ForRevisionsOfEntity(typeof (PersonAbsence), false, true)
				.AddProjection(AuditEntity.RevisionNumber().Distinct())
				.Add(AuditEntity.Property("Person").Eq(agent))
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Lt(dateTimePeriod.EndDateTime))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Gt(dateTimePeriod.StartDateTime))
				.AddOrder(AuditEntity.RevisionNumber().Desc())
				.SetMaxResults(maxSize)
				.GetResultList<long>();
		}

		private IEnumerable<IRevision> loadRevisions(IEnumerable<long> ids, int maxSize)
		{
			//flytta till revisionrepository? Gör det vid nästa sånt här historyrepository...
			var cutIds = (from id in ids orderby id descending 
								select id).Take(maxSize).ToArray();

			return session().CreateCriteria<Revision>()
				.Add(Restrictions.In("Id", cutIds))
				.AddOrder(Order.Desc("Id"))
				.List<IRevision>();
		}

		private static DateTimePeriod convertFromDateOnly(IPerson agent, DateOnly date)
		{
			return date.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone());
		}

		private ISession session()
		{
			return _unitOfWorkFactory.Session();
		}
	}
}
