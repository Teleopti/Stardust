using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public interface IShiftTradeRequestAvailability
	{
		bool IsEnabledInWebRequest();
	}

	public class ShiftTradeRequestAvailability: IShiftTradeRequestAvailability
	{
		private readonly ILicenseAvailability _licenseAvailability;

		public ShiftTradeRequestAvailability(ILicenseAvailability licenseAvailability)
		{
			_licenseAvailability = licenseAvailability;
		}

		public bool IsEnabledInWebRequest()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccShiftTrader);
		}
	}
}
