using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface IAlarmControlView
    {
        void ShowThisItem(int alarmListItemId);
        void Warning(string message);
    }
}