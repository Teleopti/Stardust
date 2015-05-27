using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.StartupConfiguration
{
	public class StartupConfigurationModel
	{
		private readonly IConfigurationHandler _configurationHandler;
		private ReadOnlyCollection<LookupIntegerItem> _cultureList;
		private ReadOnlyCollection<LookupIntegerItem> _intervalLengthList;
		private ReadOnlyCollection<LookupStringItem> _timeZoneList;
		private int? _intervalLengthAlreadyInUse;

		public StartupConfigurationModel(IConfigurationHandler configurationHandler)
		{
			_configurationHandler = configurationHandler;
		}

		public ReadOnlyCollection<LookupIntegerItem> CultureList
		{
			get
			{
				if (_cultureList == null)
				{
					CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
					var list = cultureInfos.Select(cultureInfo => new LookupIntegerItem(cultureInfo.LCID, cultureInfo.DisplayName)).ToList();
					list.Sort((c1, c2) => string.Compare(c1.Text, c2.Text, StringComparison.CurrentCulture));
					_cultureList = new ReadOnlyCollection<LookupIntegerItem>(list);
				}

				return _cultureList;
			}
		}

		public IBaseConfiguration OriginalConfiguration
		{
			get { return _configurationHandler.BaseConfiguration; }
		}

		public ReadOnlyCollection<LookupIntegerItem> IntervalLengthList
		{
			get
			{
				if (_intervalLengthList == null)
				{
					var list = new List<LookupIntegerItem>
					           	{
					           		new LookupIntegerItem(10, "10 minutes"),
					           		new LookupIntegerItem(15, "15 minutes"),
									new LookupIntegerItem(30, "30 minutes"),
									new LookupIntegerItem(60, "60 minutes")
					           	};
					_intervalLengthList = new ReadOnlyCollection<LookupIntegerItem>(list);
				}

				return _intervalLengthList;
			}
		}

		public ReadOnlyCollection<LookupStringItem> TimeZoneList
		{
			get
			{
				if (_timeZoneList == null)
				{
					var list = TimeZoneInfo.GetSystemTimeZones().Select(timeZone => new LookupStringItem(timeZone.Id, timeZone.DisplayName)).ToList();
					_timeZoneList = new ReadOnlyCollection<LookupStringItem>(list);
				}
				return _timeZoneList;
			}
		}

		public int? IntervalLengthAlreadyInUse
		{
			get
			{
				if (!_intervalLengthAlreadyInUse.HasValue)
				{
					_intervalLengthAlreadyInUse = _configurationHandler.IntervalLengthInUse;
				}

				return _intervalLengthAlreadyInUse;
			}
		}

		public void SaveConfiguration(IBaseConfiguration configuration)
		{
			_configurationHandler.SaveBaseConfiguration(configuration);
		}

		public LookupIntegerItem GetCultureItem(CultureInfo culture)
		{
			return CultureList.FirstOrDefault(cultureItem => culture.LCID == cultureItem.Id);
		}

		public LookupStringItem GetTimeZoneItem(TimeZoneInfo timeZone)
		{
			return TimeZoneList.FirstOrDefault(t => t.Id == timeZone.Id);
		}
	}
}