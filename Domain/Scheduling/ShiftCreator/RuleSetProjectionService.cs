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





	[Serializable]
	public class WorkShiftProjection : IWorkShiftProjection
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IWorkShiftProjection FromWorkShift(IWorkShift workShift)
		{
			return new WorkShiftProjection
			       	{
			       		ContractTime = workShift.Projection.ContractTime(),
			       		ShiftCategoryId = workShift.ShiftCategory.Id.Value,
			       		TimePeriod = workShift.ToTimePeriod().Value,
			       		Layers = (from l in workShift.Projection
			       		          let payloadId = l.Payload.Id ?? Guid.Empty
								  select new WorkShiftProjectionLayer
			       		                 	{
			       		                 		ActivityId = payloadId,
			       		                 		Period = l.Period
			       		                 	}
			       		         ).ToArray()
			       	};
		}

		public TimeSpan ContractTime { get; set; }
		public TimePeriod TimePeriod { get; set; }
		public Guid ShiftCategoryId { get; set; }
		public IEnumerable<IActivityRestrictableVisualLayer> Layers { get; set; }
	}

	[Serializable]
	public class WorkShiftProjectionLayer : IActivityRestrictableVisualLayer
	{
		public Guid ActivityId { get; set; }
		public DateTimePeriod Period { get; set; }
	}

}