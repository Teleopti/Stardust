﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccMyTeamLicenseOption : LicenseOption
	{
		public TeleoptiCccMyTeamLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccMyTeam, DefinedLicenseOptionNames.TeleoptiCccMyTeam)
		{
			
		}

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{

		}
	}
}