using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeLicensedFunctionProvider : ILicensedFunctionsProvider
	{
		private static readonly DefinedRaptorApplicationFunctionFactory _functions = new DefinedRaptorApplicationFunctionFactory();
		
		private readonly List<IApplicationFunction> _licensedFunctions = new List<IApplicationFunction>
		{
			ApplicationFunction.FindByPath(_functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules),
			ApplicationFunction.FindByPath(_functions.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules),
			ApplicationFunction.FindByPath(_functions.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)
		};

		public IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName)
		{
			return _licensedFunctions;
		}

		public void ClearLicensedFunctions(string tenantName)
		{
			_licensedFunctions.Clear();
		}

		public void SetLicensedFunctions(params IApplicationFunction[] functions)
		{
			_licensedFunctions.AddRange(functions);
		}
	}
}