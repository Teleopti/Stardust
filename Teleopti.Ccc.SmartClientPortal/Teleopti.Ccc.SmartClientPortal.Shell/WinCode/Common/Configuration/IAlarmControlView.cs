namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface IAlarmControlView
    {
        void ShowThisItem(int alarmListItemId);
        void Warning(string message);
	    void RefreshRow(int rowIndex);
    }
}