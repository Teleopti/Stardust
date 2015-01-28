using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class ApplicationFunctionsToggleFilter : IApplicationFunctionsToggleFilter
	{
		private readonly IApplicationFunctionsProvider _applicationFunctionsProvider;
		private readonly IToggleManager _toggleManager;

		public ApplicationFunctionsToggleFilter(IApplicationFunctionsProvider applicationFunctionsProvider, IToggleManager toggleManager)
		{
			_applicationFunctionsProvider = applicationFunctionsProvider;
			_toggleManager = toggleManager;
		}

		public AllFunctions FilteredFunctions()
		{
			var functions = _applicationFunctionsProvider.AllFunctions();
			if (!_toggleManager.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254))
			{
				hideMyReportQueueMetrics(functions);
			}
			if (!_toggleManager.IsEnabled (Toggles.SeatPlanner_32003))
			{
				hideSeatPlanner (functions);
			}

			return functions;
		}

		private static void hideMyReportQueueMetrics(AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.MyReportQueueMetrics);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}

		private static void hideSeatPlanner (AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.SeatPlanner);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}
	}
}