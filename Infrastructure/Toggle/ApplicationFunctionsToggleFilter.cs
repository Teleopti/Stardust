using System.Linq;
using Teleopti.Ccc.Domain.Common;
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
		private ICurrentDataSource _currentDataSource;

		public ApplicationFunctionsToggleFilter(IApplicationFunctionsProvider applicationFunctionsProvider, IToggleManager toggleManager, ICurrentDataSource currentDataSource)
		{
			_applicationFunctionsProvider = applicationFunctionsProvider;
			_toggleManager = toggleManager;
			_currentDataSource = currentDataSource;
		}

		public AllFunctions FilteredFunctions()
		{
			var functions = _applicationFunctionsProvider.AllFunctions();

			hideRealTimeReports(functions);
			if (!_toggleManager.IsEnabled(Toggles.WFM_Gamification_Permission_76546)) hideGamification(functions);
			if (!_toggleManager.IsEnabled(Toggles.WFM_ChatBot_77547)) hideChatBot(functions);
			
			hideBpoExchangeIfNotLicensed(functions);
			return functions;
		}

		private void hideRealTimeReports(AllFunctions functions)
		{
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_AuditTrail_44006))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport);
				foundFunction?.SetHidden();
			}
			if (!_toggleManager.IsEnabled(Toggles.WFM_AuditTrail_44006))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport);
				foundFunction?.SetHidden();
			}
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport);
				foundFunction?.SetHidden();
			}
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport);
				foundFunction?.SetHidden();
			}
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

		private void hideBpoExchangeIfNotLicensed(AllFunctions functions)
		{
			var currentName = _currentDataSource.CurrentName();
			var isLicenseAvailible = DefinedLicenseDataFactory.HasLicense(currentName) &&
									 DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(
										 DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBpoExchange);
			if (!isLicenseAvailible)
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.BpoExchange);
				foundFunction?.SetHidden();
			}

		}

		private void hideGamification(AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.Gamification);
			foundFunction?.SetHidden();
		}

		private void hideChatBot(AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ChatBot);
			foundFunction?.SetHidden();
		}
	}
}