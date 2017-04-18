using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked();
		void BackButtonClicked();
		void Initialize();
		bool Start(string raptorServer);
		LoginStep CurrentStep { get; set; }
		bool webLogin(Guid parse);
	}
}