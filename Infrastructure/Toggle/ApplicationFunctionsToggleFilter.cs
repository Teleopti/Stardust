using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

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

			if (!_toggleManager.IsEnabled(Toggles.MyTimeWeb_OvertimeRequest_44558)) hideOvertimeRequest(functions);

			hideBpoExchangeIfNotLicensed(functions);
			return functions;
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

		private static void hideOvertimeRequest(AllFunctions functions)
		{
			var foundFunction = functions.FindByForeignId(DefinedRaptorApplicationFunctionForeignIds.OvertimeRequestsWeb);
			if (foundFunction != null)
			{
				foundFunction.SetHidden();
			}
		}
	}
}