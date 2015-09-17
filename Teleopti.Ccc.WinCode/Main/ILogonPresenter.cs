using System;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked();
		void BackButtonClicked();
		void Initialize();
		bool Start(string raptorServer);
		LoginStep CurrentStep { get; set; }
		bool IdLogin(Guid parse);
	}
}