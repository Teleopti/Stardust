using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILoginWebView : ILicenseFeedback
	{
		bool StartLogon(string raptorServer);
	}
}