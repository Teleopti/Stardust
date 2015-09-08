using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class CurrentScheduleSummaryCalculatorTest
	{
		private MockRepository _mocks;
		private CurrentScheduleSummaryCalculator _target;
		private IScheduleRange _scheduleRange;
		private IScheduleDictionary _dic;
		private IPerson _person;
		private IScheduleDateTimePeriod _scheduleDateTimePeriod;
		private IScheduleDay _scheduleDay1;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new CurrentScheduleSummaryCalculator();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod(2015,5,20,2015,5,20));
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldCountContractTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleRange.Person).Return(_person);
				Expect.Call(_scheduleRange.Owner).Return(_dic);
				Expect.Call(_dic.Period).Return(_scheduleDateTimePeriod);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2015, 5, 20))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
			}

			using (_mocks.Playback())
			{
				var result = _target.GetCurrent(_scheduleRange);
				Assert.AreEqual(TimeSpan.FromHours(8), result.Item1);
				Assert.AreEqual(0, result.Item2);
			}
		}

		[Test]
		public void ShouldCountContractTimeOnPeriod()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleRange.Person).Return(_person);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2015, 5, 20))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
			}

			using (_mocks.Playback())
			{
				var dateOnlyPeriod = _scheduleDateTimePeriod.LoadedPeriod().ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
				var result = _target.GetCurrent(_scheduleRange, dateOnlyPeriod);
				Assert.AreEqual(TimeSpan.FromHours(8), result.Item1);
				Assert.AreEqual(0, result.Item2);
			}	
		}

		[Test]
		public void ShouldCountDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleRange.Person).Return(_person);
				Expect.Call(_scheduleRange.Owner).Return(_dic);
				Expect.Call(_dic.Period).Return(_scheduleDateTimePeriod);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2015, 5, 20))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var result = _target.GetCurrent(_scheduleRange);
				Assert.AreEqual(TimeSpan.Zero, result.Item1);
				Assert.AreEqual(1, result.Item2);
			}
		}
	}
}