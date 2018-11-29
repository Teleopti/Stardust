using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class AbsencePreferenceFullDayLayerCreatorTest
	{
		private AbsencePreferenceFullDayLayerCreator _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private DateTimePeriod _period;
		private DateTimePeriod _expectedPeriod;
		private IAbsence _absence;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var date = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_period = new DateTimePeriod(date, date.AddDays(1));
			_expectedPeriod = new DateTimePeriod(_period.StartDateTime.AddHours(8), _period.StartDateTime.AddHours(20));
			_absence = _mocks.StrictMock<IAbsence>();
			_target = new AbsencePreferenceFullDayLayerCreator();	
		}

		[Test]
		public void ShouldCreateAbsenceLayer()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.Period).Return(_period);
			}

			using (_mocks.Playback())
			{
				var absenceLayer = _target.Create(_scheduleDay, _absence);
				Assert.AreEqual(_expectedPeriod, absenceLayer.Period);
				Assert.AreEqual(_absence, absenceLayer.Payload);
			}
		}
	}
}
