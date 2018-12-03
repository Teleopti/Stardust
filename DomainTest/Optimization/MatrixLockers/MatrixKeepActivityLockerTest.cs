using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Optimization.MatrixLockers
{
	[TestFixture]
	public class MatrixKeepActivityLockerTest
	{
		private MatrixKeepActivityLocker _target;
		private MockRepository _mock;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros;
		private IActivity _activity;
		private IList<IActivity> _activities;
		private DateOnly _dateOnly;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDayPro[] _unlockedScheduleDayPros;
		private IScheduleDay _scheduleDay;
		private DateTimePeriod _period;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private IVisualLayer _visualLayer;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_activity = new Activity("activity");
			_activities = new List<IActivity>{_activity};
			_target = new MatrixKeepActivityLocker(_scheduleMatrixPros, _activities);
			_dateOnly = new DateOnly(2014, 1, 1);
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_unlockedScheduleDayPros = new []{_scheduleDayPro};
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_period = new DateTimePeriod(new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayer = new VisualLayer(_activity, _period, _activity);
			_visualLayerCollection = new VisualLayerCollection(new List<IVisualLayer>{_visualLayer}, new ProjectionPayloadMerger());
		}

		[Test]
		public void ShouldLockMatrixesHavingKeepActivity()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(_unlockedScheduleDayPros);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasProjection()).Return(true);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(() => _scheduleMatrixPro.LockDay(_dateOnly));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}
		}
	}
}
