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

		public ApplicationFunctionsToggleFilter(IApplicationFunctionsProvider applicationFunctionsProvider,
			IToggleManager toggleManager)
		{
			_applicationFunctionsProvider = applicationFunctionsProvider;
			_toggleManager = toggleManager;
		}

		public AllFunctions FilteredFunctions()
		{
			var functions = _applicationFunctionsProvider.AllFunctions();

			hideRealTimeReports(functions);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_ChatBot_77547,
				DefinedRaptorApplicationFunctionForeignIds.ChatBot);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Gamification_Permission_76546,
				DefinedRaptorApplicationFunctionForeignIds.Gamification);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Request_View_Permissions_77731,
				DefinedRaptorApplicationFunctionForeignIds.WebApproveOrDenyRequest);

			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.BpoExchange);
			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.ChatBot);

			hidePmNextGen(functions);

			return functions;
		}

		private void hidePmNextGen(AllFunctions functions)
		{
			hideIfNotLicensed(functions,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGen,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGenViewReport,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGenEditReport);

			hideAppFunctionWithToggleOff(functions, Toggles.Wfm_PmNextGen_78059,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGen,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGenViewReport,
				DefinedRaptorApplicationFunctionForeignIds.PmNextGenEditReport);
		}

		private void hideRealTimeReports(AllFunctions functions)
		{
			hideAppFunctionWithToggleOn(functions, Toggles.Report_Remove_Realtime_AuditTrail_44006,
				DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_AuditTrail_44006,
				DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport);
			hideAppFunctionWithToggleOn(functions, Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560,
				DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport);
			hideAppFunctionWithToggleOn(functions, Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,
				DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport);

			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_AuditTrail_44006))
			{
				foreach (var function in functions.Functions)
				{
					var onlineReportNode = function.ChildFunctions.FirstOrDefault(x =>
						x.Function.LocalizedFunctionDescription == Resources.OnlineReports);
					onlineReportNode?.SetHidden();
				}
			}
		}

		private void hideIfNotLicensed(AllFunctions functions, params string[] applicationFunctionForeignIds)
		{
			foreach (var foreignId in applicationFunctionForeignIds)
			{
				var foundFunction = functions.FindByForeignId(foreignId);
				if (foundFunction != null && !foundFunction.IsLicensed)
				{
					foundFunction.SetHidden();
				}
			}
		}

		private void hideAppFunctionWithToggleOff(AllFunctions functions, Toggles toggle, params string[] appFunctions)
		{
			if (_toggleManager.IsEnabled(toggle)) return;

			foreach (var appFunction in appFunctions)
			{
				var foundFunction = functions.FindByForeignId(appFunction);
				foundFunction?.SetHidden();
			}
		}

		private void hideAppFunctionWithToggleOn(AllFunctions functions, Toggles toggle, params string[] appFunctions)
		{
			if (!_toggleManager.IsEnabled(toggle)) return;

			foreach (var appFunction in appFunctions)
			{
				var foundFunction = functions.FindByForeignId(appFunction);
				foundFunction?.SetHidden();
			}
		}
	}
}