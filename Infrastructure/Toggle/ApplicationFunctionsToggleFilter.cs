using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
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
		private readonly IOvertimeRequestAvailability _overtimeRequestLicense;

		public ApplicationFunctionsToggleFilter(IApplicationFunctionsProvider applicationFunctionsProvider, IToggleManager toggleManager, ICurrentDataSource currentDataSource, IOvertimeRequestAvailability overtimeRequestLicense)
		{
			_applicationFunctionsProvider = applicationFunctionsProvider;
			_toggleManager = toggleManager;
			_currentDataSource = currentDataSource;
			_overtimeRequestLicense = overtimeRequestLicense;
		}

		public AllFunctions FilteredFunctions()
		{
			var functions = _applicationFunctionsProvider.AllFunctions();

			hideOvertimeRequest(functions);
			hideRealTimeReports(functions);

			hideBpoExchangeIfNotLicensed(functions);
			return functions;
		}

		private void hideRealTimeReports(AllFunctions functions)
		{
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_AuditTrail_44006))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport);
				if (foundFunction != null)
				{
					foundFunction.SetHidden();
				}
			}
			if (!_toggleManager.IsEnabled(Toggles.WFM_AuditTrail_44006))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport);
				if (foundFunction != null)
				{
					foundFunction.SetHidden();
				}
			}
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport);
				if (foundFunction != null)
				{
					foundFunction.SetHidden();
				}
			}
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559))
			{
				var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport);
				if (foundFunction != null)
				{
					foundFunction.SetHidden();
				}
			}
			if (_toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560)
				&& _toggleManager.IsEnabled(Toggles.Report_Remove_Realtime_AuditTrail_44006))
			{
				foreach (var function in functions.Functions)
				{
					var onlineReportNode = function.ChildFunctions.FirstOrDefault(x => x.Function.LocalizedFunctionDescription == Resources.OnlineReports);
					if (onlineReportNode != null)
					{
						onlineReportNode.SetHidden();
					}
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
				if (foundFunction != null)
				{
					foundFunction.SetHidden();
				}
			}

		}

		private void hideOvertimeRequest(AllFunctions functions)
		{
			if (_overtimeRequestLicense.IsEnabled() && _toggleManager.IsEnabled(Toggles.MyTimeWeb_OvertimeRequest_44558)) return;

			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.OvertimeRequestsWeb);
			foundFunction?.SetHidden();
		}
	}
}