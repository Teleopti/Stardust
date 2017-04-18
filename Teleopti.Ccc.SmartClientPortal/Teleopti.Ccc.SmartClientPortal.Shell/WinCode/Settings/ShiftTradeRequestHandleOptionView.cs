using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
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
