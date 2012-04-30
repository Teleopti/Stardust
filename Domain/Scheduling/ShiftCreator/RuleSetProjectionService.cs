using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class RuleSetProjectionService : IRuleSetProjectionService
	{
		private readonly IShiftCreatorService _shiftCreatorService;

		public RuleSetProjectionService(IShiftCreatorService shiftCreatorService)
		{
			_shiftCreatorService = shiftCreatorService;
		}

		public IEnumerable<IWorkShiftProjection> ProjectionCollection(IWorkShiftRuleSet workShiftRuleSet)
		{
			return (
			       	from s in _shiftCreatorService.Generate(workShiftRuleSet)
			       	select WorkShiftProjection.FromWorkShift(s)
			       ).ToArray();
		}
	}






	public class WorkShiftProjection : IWorkShiftProjection
	{

		public static IWorkShiftProjection FromWorkShift(IWorkShift workShift)
		{
			return new WorkShiftProjection
			       	{
			       		ContractTime = workShift.Projection.ContractTime(),
			       		ShiftCategoryId = workShift.ShiftCategory.Id.Value,
			       		TimePeriod = workShift.ToTimePeriod(),
			       		Layers = (from l in workShift.Projection
			       		          let payloadId = l.Payload.Id ?? Guid.Empty
			       		          select new ActivityRestrictableVisualLayer
			       		                 	{
			       		                 		ActivityId = payloadId,
			       		                 		Period = l.Period
			       		                 	}
			       		         ).ToArray()
			       	};
		}

		public TimeSpan ContractTime { get; set; }
		public TimePeriod? TimePeriod { get; set; }
		public Guid ShiftCategoryId { get; set; }
		public IEnumerable<IActivityRestrictableVisualLayer> Layers { get; set; }
	}

	public class ActivityRestrictableVisualLayer : IActivityRestrictableVisualLayer
	{
		public Guid ActivityId { get; set; }
		public DateTimePeriod Period { get; set; }
	}

}