using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ShiftCategoryUsageFinder : IShiftCategoryUsageFinder
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ShiftCategoryUsageFinder(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<ShiftCategoryExample> Find()
		{
			var result = _currentUnitOfWork.Current().Session().CreateSQLQuery(@"SELECT p.ShiftCategory
					,[BelongsToDate]
				, CAST(DATEDIFF(mi, BelongsToDate, StartDateTime) as decimal(14, 2)) / 60.0 as [StartTime]
				, CAST(DATEDIFF(mi, BelongsToDate, EndDateTime) as decimal(14, 2)) / 60.0 as [EndTime]
			FROM [ReadModel].[ScheduleDay] s WITH(NOLOCK)
			INNER JOIN[dbo].PersonAssignment p WITH(NOLOCK) ON p.Date = s.BelongsToDate AND p.Person = s.PersonId
			INNER JOIN[dbo].ShiftCategory sc WITH(NOLOCK) ON sc.id = p.ShiftCategory
			INNER JOIN[dbo].Scenario scenario WITH(NOLOCK) ON scenario.Id = p.Scenario
			WHERE sc.IsDeleted = 0
			AND scenario.DefaultScenario = 1
			AND scenario.BusinessUnit = :businessUnit
			AND s.BelongsToDate > DATEADD(d, -30, GETDATE())")
				.SetGuid("businessUnit", ServiceLocator_DONTUSE.CurrentBusinessUnit.CurrentId().GetValueOrDefault())
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(shiftCategoryPredictIntermediateResult)))
				.List<shiftCategoryPredictIntermediateResult>();

			return result.Select(s => new ShiftCategoryExample
			{
				DayOfWeek = s.BelongsToDate.DayOfWeek,
				StartTime = (double) s.StartTime,
				EndTime = (double) s.EndTime,
				ShiftCategory = s.ShiftCategory.ToString()
			});
		}

		private class shiftCategoryPredictIntermediateResult
		{
			public Guid ShiftCategory { get; set; }
			public DateTime BelongsToDate { get; set; }
			public decimal StartTime { get; set; }
			public decimal EndTime { get; set; }
		}
	}
}