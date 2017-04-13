using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
	public class ShiftTradeRequestHandleOptionView
	{
		public string Description { get; }

		public RequestHandleOption RequestHandleOption { get; }

		public ShiftTradeRequestHandleOptionView(RequestHandleOption option, string description)
		{
			RequestHandleOption = option;
			Description = description;
		}
	}
}
