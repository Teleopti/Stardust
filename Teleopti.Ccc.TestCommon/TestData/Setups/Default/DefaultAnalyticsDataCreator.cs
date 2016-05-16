using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultAnalyticsDataCreator
	{
		private static readonly Func<IList<IAnalyticsDataSetup>> generateSetups = () =>
		{
			var utcAndCetTimeZones = new UtcAndCetTimeZones();
			var existingDatasources = new ExistingDatasources(utcAndCetTimeZones);
			var quarterOfAnHourInterval = new QuarterOfAnHourInterval();
			var datesFromPeriod = new DatesFromPeriod(new DateTime(2001, 1, 1), new DateTime(2035, 1, 1));
			var sysConfiguration = new SysConfiguration("IntervalLengthMinutes", "15");
			sysConfiguration.AddConfiguration("TimeZoneCode", "W. Europe Standard Time");
			return new IAnalyticsDataSetup[]
			{
				utcAndCetTimeZones,
				existingDatasources,
				datesFromPeriod,
				new BusinessUnit(DefaultBusinessUnit.BusinessUnit, existingDatasources),
				new EternityAndNotDefinedDate(),
				sysConfiguration,
				quarterOfAnHourInterval,
				new DefaultSkillset(),
				new DefaultAcdLogin(),
				new Scenario(12, DefaultBusinessUnit.BusinessUnit.Id.GetValueOrDefault(), true),
				Team.NotDefinedTeam(),
				Person.NotDefinedPerson(existingDatasources)
			};
		};
		private static readonly IList<IAnalyticsDataSetup> setups = generateSetups();

		private int? _hashValue;

		public int HashValue
		{
			get
			{
				if (!_hashValue.HasValue)
					_hashValue = setups.Aggregate(37, (current, setup) => current ^ setup.GetHashCode());
				return _hashValue.Value;
			}
		}

		public void Create()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			foreach (var setup in setups)
			{
				analyticsDataFactory.Apply(setup);
			}
			analyticsDataFactory.Persist();
		}

		public static int GetDateId(DateTime date)
		{
			return (int)GetDateRow(date)["date_id"];
		}

		public static DataRow GetDateRow(DateTime date)
		{
			return setups.OfType<DatesFromPeriod>().First().DateMap[date.Date];
		}

		public static ITimeZoneData GetTimeZones()
		{
			return setups.OfType<UtcAndCetTimeZones>().First();
		}

		public static IIntervalData GetInterval()
		{
			return setups.OfType<QuarterOfAnHourInterval>().First();
		}

		public static IDatasourceData GetDataSources()
		{
			return setups.OfType<ExistingDatasources>().First();
		}
	}
}