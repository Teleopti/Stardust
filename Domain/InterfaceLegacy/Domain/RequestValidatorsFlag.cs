using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[Flags]
	public enum RequestValidatorsFlag
	{
		None = 0,
		BudgetAllotmentValidator = 1 << 0,
		IntradayValidator = 1 << 1,
		ExpirationValidator = 1 << 2
	}
}