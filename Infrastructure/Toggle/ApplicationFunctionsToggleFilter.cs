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
			if (!_toggleManager.IsEnabled (Toggles.SeatPlanner_Logon_32003))
			{
				hideSeatPlanner (functions);
			}
			if (!_toggleManager.IsEnabled(Toggles.Wfm_Outbound_Campaign_32696))
			{
				hideOutbound(functions);
			}
			if(!_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_SwapShifts_36231))
			{
				hideWfmTeamScheduleSwapShifts(functions);
			}

			return functions;
		}

		private static void hideSeatPlanner (AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.SeatPlanner);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}
		
		private static void hideOutbound (AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.Outbound);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}

		private static void hideWfmTeamScheduleSwapShifts(AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.SwapShifts);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}
	}
}