using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	[Flags]
	public enum RequestValidatorsFlag
	{
		None = 0,
		BudgetAllotmentValidator = 1 << 0,
		IntradayValidator = 1 << 1,
	}
}