using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeConfigurationHandler : IConfigurationHandler
	{
		public string connectionString;

		readonly IDictionary<string, IBaseConfiguration> _baseConfigurationDic = new ConcurrentDictionary<string, IBaseConfiguration>();

		public bool IsConfigurationValid {
			get
			{
				if (!isCultureValid(BaseConfiguration.CultureId))
					return false;
				if (!isIntervalLengthValid(BaseConfiguration.IntervalLength))
					return false;

				return isTimeZoneValid(BaseConfiguration.TimeZoneCode);
			}
		}

		public IBaseConfiguration BaseConfiguration
		{
			get
			{
				if (_baseConfigurationDic.ContainsKey(connectionString))
					return _baseConfigurationDic[connectionString];
				return new BaseConfiguration(null, null, null, false);
			}
		}

		public int? IntervalLengthInUse { get; }
		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			throw new NotImplementedException();
		}

		public void SetConnectionString(string dataMartConnectionString)
		{
			connectionString = dataMartConnectionString;
		}

		private bool isCultureValid(int? uiCultureId)
		{
			if (!uiCultureId.HasValue)
			{
				return false;
			}

			return true;
		}

		private bool isIntervalLengthValid(int? intervalLength)
		{
			if (!intervalLength.HasValue)
			{
				return false;
			}

			if (intervalLength != 10 & intervalLength != 15 & intervalLength != 30 & intervalLength != 60)
			{
				return false;
			}

			return true;
		}

		private bool isTimeZoneValid(string timeZone)
		{
			if (string.IsNullOrEmpty(timeZone))
			{
				return false;
			}
			return true;
		}

		public void AddBaseConfiguration(string connectionString, IBaseConfiguration configuration)
		{
			if (_baseConfigurationDic.ContainsKey(connectionString))
			{
				_baseConfigurationDic[connectionString] = configuration;
			}
			else
			{
				_baseConfigurationDic.Add(connectionString, configuration);
			}
		}
	}
}
