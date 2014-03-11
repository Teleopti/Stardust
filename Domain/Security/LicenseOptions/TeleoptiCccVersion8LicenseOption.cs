using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccVersion8LicenseOption : LicenseOption
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TeleoptiCccShiftTradesLicenseOption"/> class.
		/// </summary>
		public TeleoptiCccVersion8LicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccVersion8, DefinedLicenseOptionNames.TeleoptiCccVersion8)
		{
		}

		/// <summary>
		/// Sets all application functions.
		/// </summary>
		/// <param name="allApplicationFunctions"></param>
		/// <value>The enabled application functions.</value>
		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();

			var function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.Anywhere);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.IntradayReForecasting);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.MonthSchedule);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);

			function = ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			function.IsPreliminary = false;
			EnabledApplicationFunctions.Add(function);
		}
	}
}