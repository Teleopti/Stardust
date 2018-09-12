using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;

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

			hideRealTimeReports(functions);
			hideAppFunctionWithToggleOff(functions, DefinedRaptorApplicationFunctionForeignIds.ChatBot, Toggles.WFM_ChatBot_77547);
			hideAppFunctionWithToggleOff(functions, DefinedRaptorApplicationFunctionForeignIds.Gamification, Toggles.WFM_Gamification_Permission_76546);
			
			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.BpoExchange);
			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.ChatBot);
			return functions;
		}

		private void hideRealTimeReports(AllFunctions functions)
		{
			hideAppFunctionWithToggleOn(functions, DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport,
				Toggles.Report_Remove_Realtime_AuditTrail_44006);
			hideAppFunctionWithToggleOff(functions, DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport,
				Toggles.WFM_AuditTrail_44006);
			hideAppFunctionWithToggleOn(functions, DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport,
				Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560);
			hideAppFunctionWithToggleOn(functions, DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport,
				Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559);

			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_AuditTrail_44006))
			{
				foreach (var function in functions.Functions)
				{
					var onlineReportNode = function.ChildFunctions.FirstOrDefault(x => x.Function.LocalizedFunctionDescription == Resources.OnlineReports);
					onlineReportNode?.SetHidden();
				}
			}
		}

		private void hideIfNotLicensed(AllFunctions functions, string applicationFunctionForeignId)
		{
			var foundFunction = functions.FindByForeignId(applicationFunctionForeignId);
			if (foundFunction!=null && !foundFunction.IsLicensed)
			{
				foundFunction.SetHidden();
			}
		}
		
		private void hideAppFunctionWithToggleOff(AllFunctions functions, string appFunction, Toggles toggle)
		{
			if (_toggleManager.IsEnabled(toggle)) return;
			var foundFunction = functions.FindByForeignId(appFunction);
			foundFunction?.SetHidden();
		}

		private void hideAppFunctionWithToggleOn(AllFunctions functions, string appFunction, Toggles toggle)
		{
			if (!_toggleManager.IsEnabled(toggle)) return;
			var foundFunction = functions.FindByForeignId(appFunction);
			foundFunction?.SetHidden();
		}
	}
}