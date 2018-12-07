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
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Request_View_Permissions_77731,
				DefinedRaptorApplicationFunctionForeignIds.WebReplyRequest);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Request_View_Permissions_77731,
				DefinedRaptorApplicationFunctionForeignIds.WebEditSiteOpenHours);
			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Connect_NewLandingPage_WEB_78578,
				DefinedRaptorApplicationFunctionForeignIds.ViewCustomerCenter);

			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.BpoExchange);
			hideIfNotLicensed(functions, DefinedRaptorApplicationFunctionForeignIds.ChatBot);

			hideInsights(functions);

			return functions;
		}

		private void hideInsights(AllFunctions functions)
		{
			hideIfNotLicensed(functions,
				DefinedRaptorApplicationFunctionForeignIds.Insights,
				DefinedRaptorApplicationFunctionForeignIds.DeleteInsightsReport,
				DefinedRaptorApplicationFunctionForeignIds.EditInsightsReport);

			hideAppFunctionWithToggleOff(functions, Toggles.WFM_Insights_78059,
				DefinedRaptorApplicationFunctionForeignIds.Insights,
				DefinedRaptorApplicationFunctionForeignIds.DeleteInsightsReport,
				DefinedRaptorApplicationFunctionForeignIds.EditInsightsReport);
		}

		private void hideRealTimeReports(AllFunctions functions)
		{
			hideAppFunctionWithToggleOff(functions, Toggles.Wfm_AuditTrail_StaffingAuditTrail_78125,
				DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport);
			hideAppFunctionWithToggleOn(functions, Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,
				DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport);

			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559))
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