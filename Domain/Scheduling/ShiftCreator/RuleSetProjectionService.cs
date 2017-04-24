using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class RuleSetProjectionService : IRuleSetProjectionService
	{
		private readonly IShiftCreatorService _shiftCreatorService;

		public RuleSetProjectionService(IShiftCreatorService shiftCreatorService)
		{
			_shiftCreatorService = shiftCreatorService;
		}

		public IEnumerable<IWorkShiftProjection> ProjectionCollection(IWorkShiftRuleSet workShiftRuleSet, IWorkShiftAddCallback callback)
		{
			return (
					from col in _shiftCreatorService.Generate(workShiftRuleSet, callback)
					from s in col
			       	select WorkShiftProjection.FromWorkShift(s)
			       ).ToArray();
		}
	}

}