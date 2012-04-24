using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference
{
	public class RuleSetProjectionServiceForMultiSessionCaching : IRuleSetProjectionService
	{
		private readonly IRuleSetProjectionService _service;
		private readonly ILazyLoadingManager _lazyLoadingManager;

		public RuleSetProjectionServiceForMultiSessionCaching(IRuleSetProjectionService service, ILazyLoadingManager lazyLoadingManager)
		{
			_service = service;
			_lazyLoadingManager = lazyLoadingManager;
		}

		public virtual IEnumerable<IWorkShiftVisualLayerInfo> ProjectionCollection(IWorkShiftRuleSet ruleSet)
		{
			var result = _service.ProjectionCollection(ruleSet);
			if (result == null)
				return null;
			result.ForEach(r =>
			               	{
								_lazyLoadingManager.Initialize(r.WorkShift);
								_lazyLoadingManager.Initialize(r.WorkShift.ShiftCategory);
								_lazyLoadingManager.Initialize(r.VisualLayerCollection);
			               		var visualLayers = r.VisualLayerCollection.OfType<VisualLayer>();
			               		visualLayers.ForEach(l => _lazyLoadingManager.Initialize(l.HighestPriorityActivity));
							});
			return result;
		}
	}
}