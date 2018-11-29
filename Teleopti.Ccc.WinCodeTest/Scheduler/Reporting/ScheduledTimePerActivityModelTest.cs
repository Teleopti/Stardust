using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.WinCodeTest.Common;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduledTimePerActivityModelTest
	{
		private MockRepository _mockRepository;
		private ScheduledTimePerActivityModel _target;
		private IScheduleDictionary _scheduleDictionary;
		private IPerson _person;
		private IList<DateOnly> _dateOnlies;
		private IList<IPayload> _payloads;
		private IList<IPerson> _persons; 
		private IPayload _payload;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;
		private DateOnly _dateOnly;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private IVisualLayer _visualLayer;
		private DateTimePeriod _period;
		private IActivity _activity;
		private IList<IVisualLayer> _visualLayers;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
			
		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_target = new ScheduledTimePerActivityModel();
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_person = PersonFactory.CreatePerson("Person");
			_persons = new List<IPerson>{_person};
			 _dateOnly = new DateOnly(2013, 1, 1);
			_dateOnlies = new List<DateOnly>{_dateOnly};
			_period = new DateTimePeriod(new DateTime(2013,1,1,8,0,0,DateTimeKind.Utc), new DateTime(2013,1,1,9,0,0,DateTimeKind.Utc));
			_payload = new Activity("activity");
			_payloads = new List<IPayload>{_payload};
			_scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
			_scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
			_projectionService = _mockRepository.StrictMock<IProjectionService>();
			_activity = new Activity("activity");
			_visualLayer = new VisualLayer(_payload, _period , _activity);
			_visualLayers = new List<IVisualLayer>{_visualLayer};
			_visualLayerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
			_dateOnlyAsDateTimePeriod = _mockRepository.StrictMock<IDateOnlyAsDateTimePeriod>();


		}

		[Test]
		public void ShouldSetProperties()
		{
			_target.ActivityName = "activityName";
			_target.ScheduledTime = 10;
			_target.ScheduledDate = new DateTime(10);

			Assert.AreEqual("activityName", _target.ActivityName);
			Assert.AreEqual(10, _target.ScheduledTime);
			Assert.AreEqual(new DateTime(10), _target.ScheduledDate);
		}
		
		[Test]
		public void ShouldGetReportDataFromScheduleDictionary()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly, true)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Person).Return(new Person());
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mockRepository.Playback())
			{
				var data = ScheduledTimePerActivityModel.GetReportDataFromScheduleDictionary(_scheduleDictionary, _persons, _dateOnlies, _payloads);
				Assert.AreEqual(1, data.Count);
				Assert.AreEqual(data[0].ActivityName, _activity.Description.Name);
				Assert.AreEqual(data[0].ScheduledTime, _visualLayer.Period.ElapsedTime().TotalMinutes);
				Assert.AreEqual(data[0].ScheduledDate, _dateOnly.Date);
			}
		}

		[Test]
		public void ShouldGetReportDataFromScheduleParts()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_scheduleDay.Person).Return(new Person());
			}

			using (_mockRepository.Playback())
			{
				var data = ScheduledTimePerActivityModel.GetReportDataFromScheduleParts(new List<IScheduleDay> {_scheduleDay});
				Assert.AreEqual(1, data.Count);
				Assert.AreEqual(data[0].ActivityName, _activity.Description.Name);
				Assert.AreEqual(data[0].ScheduledTime, _visualLayer.Period.ElapsedTime().TotalMinutes);
				Assert.AreEqual(data[0].ScheduledDate, _dateOnly.Date);
			}
		}

		[Test]
		public void ShouldGetReportDataFromSchedulePart()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_scheduleDay.Person).Return(new Person());
			}

			using (_mockRepository.Playback())
			{
				var data = ScheduledTimePerActivityModel.GetReportDataFromSchedulePart(_scheduleDay, _payloads);
				Assert.AreEqual(1, data.Count);
				Assert.AreEqual(data[0].ActivityName, _activity.Description.Name);
				Assert.AreEqual(data[0].ScheduledTime, _visualLayer.Period.ElapsedTime().TotalMinutes);
				Assert.AreEqual(data[0].ScheduledDate, _dateOnly.Date);
			}	
		}

		[Test]
		public void ShouldReturnNoDataIfNoMatchWhenGetReportDataFromSchedulePart()
		{
			var payload = new Activity("anotherActivity");
			_payloads = new List<IPayload> { payload };

			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
			}

			using (_mockRepository.Playback())
			{
				var data = ScheduledTimePerActivityModel.GetReportDataFromSchedulePart(_scheduleDay, _payloads);
				Assert.AreEqual(0, data.Count);
			}		
		}
	}
}
