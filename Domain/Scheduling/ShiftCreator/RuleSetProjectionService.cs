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

}