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
		private static readonly Func<IList<IAnalyticsDataSetup>> oneTimeSetups = () => new IAnalyticsDataSetup[]
		{
			new RequestStatus(0, "Pending", "ResRequestStatusPending"),
			new RequestStatus(1, "Approved", "ResRequestStatusApproved"),
			new RequestStatus(2, "Denied", "ResRequestStatusDenied"),
			new RequestType(0, "Text", "ResRequestTypeText"),
			new RequestType(1, "Absence", "ResRequestTypeAbsence"),
			new RequestType(2, "Shift Trade", "ResRequestTypeShiftTrade"),
		};

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
				new DefaultAcdLogin(-1, -1),
				new Scenario(12, DefaultBusinessUnit.BusinessUnit.Id.GetValueOrDefault(), true),
				Team.NotDefinedTeam(),
				Person.NotDefinedPerson(existingDatasources),
				Absence.NotDefinedAbsence(existingDatasources, 1),
				DimDayOff.NotDefined(existingDatasources, 1),
				ShiftCategory.NotDefined(existingDatasources, 1),
				Activity.NotDefined(existingDatasources, 1)
			};
		};
		private static readonly IList<IAnalyticsDataSetup> setups = generateSetups();

		public void OneTimeSetup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			foreach (var setup in oneTimeSetups())
			{
				try
				{
					analyticsDataFactory.Apply(setup);
				}
				catch
				{
				}
			}
			analyticsDataFactory.Persist();
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

		public static ITimeZoneData GetTimeZoneRows()
		{
			return setups.OfType<ITimeZoneData>().First();
		}

		public static ITimeZoneUtcAndCet GetTimeZoneIds()
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