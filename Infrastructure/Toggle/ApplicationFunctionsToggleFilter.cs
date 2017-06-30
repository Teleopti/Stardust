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

			if (!_toggleManager.IsEnabled(Toggles.MyTimeWeb_OvertimeRequest_44558)) hideOvertimeRequest(functions);

			return functions;
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