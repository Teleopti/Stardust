﻿using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public interface ILogonStep
	{
		void SetData(LogonModel model);
	    LogonModel GetData();
	    void Release();
	}
}
