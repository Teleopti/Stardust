using System.Collections.ObjectModel;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.StartupConfiguration
{
	public interface IStartupConfigurationView
	{
		void LoadCultureList(ReadOnlyCollection<LookupIntegerItem> cultureList);
		void SetDefaultCulture(LookupIntegerItem lookupIntegerItem);
		void LoadIntervalLengthList(ReadOnlyCollection<LookupIntegerItem> intervalLengthList);
		void SetDefaultIntervalLength(int intervalLengthMinutes);
		void SetDefaultTimeZone(LookupStringItem timeZone);
		void LoadTimeZoneList(ReadOnlyCollection<LookupStringItem> timeZoneList);
		void DisableIntervalLength();
		void DisableOkButton();
		void ShowErrorMessage(string message);
		object SelectedIntervalLengthValue { get; }
	}
}
