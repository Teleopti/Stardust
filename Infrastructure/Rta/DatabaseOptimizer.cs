using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseOptimizer : IDatabaseOptimizer
	{
		private const string agentState = "AgentState";
		private const string scheduledActivity = "ScheduledActivity"; private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;

		public int Called;

		public DatabaseOptimizer(
			ICurrentUnitOfWork unitOfWork,
			INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public StateAndSchedule LoadFor(Guid personId)
		{
			var data = get(personId);
			return new StateAndSchedule
			{
				AgentState = getResultForType<internalState>(data)?.FirstOrDefault(),
				Schedule = getResultForType<ScheduledActivity>(data)?.ToList()
			};
		}

		private static IList<T> getResultForType<T>(IEnumerable data)
		{
			return data.OfType<IList<T>>().FirstOrDefault();
		}

		private IList get(Guid personId)
		{
			var utcDate = _now.UtcDateTime().Date;

			var agentStateQuery = _unitOfWork.Current()
				   .Session()
				   .CreateSQLQuery(@"SELECT * 
					FROM [dbo].[AgentState] 
					WITH (UPDLOCK) 
					WHERE PersonId = :PersonId")
				   .SetParameter("PersonId", personId)
				   .SetResultTransformer(Transformers.AliasToBean(typeof(internalState)));

			var scheduledActivityQuery = _unitOfWork.Current()
				   .Session()
				  .CreateSQLQuery(@"SELECT 
					PayloadId,
					StartDateTime,
					EndDateTime,
					Name,
					ShortName,
					DisplayColor, 
					BelongsToDate 
					FROM ReadModel.ScheduleProjectionReadOnly
					WHERE PersonId = :PersonId
					AND BelongsToDate BETWEEN :StartDate AND :EndDate
					ORDER BY EndDateTime ASC")
				.SetParameter("PersonId", personId)
				.SetParameter("StartDate", utcDate.AddDays(-1))
				.SetParameter("EndDate", utcDate.AddDays(1))
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)));

			var multiQueryByKey = _unitOfWork.Current()
								.Session().CreateMultiQuery()
								.Add<internalState>(agentState, agentStateQuery)
								.Add<ScheduledActivity>(scheduledActivity, scheduledActivityQuery);

			return multiQueryByKey.List();
		}

		private class internalState : AgentState
		{
			public new int Adherence { set { base.Adherence = (Adherence?)value; } }
		}

		private class internalModel : ScheduledActivity
		{
			public new DateTime BelongsToDate { set { base.BelongsToDate = new DateOnly(value); } }
			public new DateTime StartDateTime { set { base.StartDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); } }
			public new DateTime EndDateTime { set { base.EndDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); } }
		}
	}
}