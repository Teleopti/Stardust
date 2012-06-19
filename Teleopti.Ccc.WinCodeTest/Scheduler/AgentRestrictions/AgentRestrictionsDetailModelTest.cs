using System;
using System.Linq;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailModelTest
	{
		private AgentRestrictionsDetailModel _model;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private MockRepository _mocks;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDay _scheduleDay;
		private IScheduleDayPro _scheduleDayPro;
		private DateTimePeriod _period;

		[SetUp]
		public void Setup()
		{
			_period = new DateTimePeriod(2012, 6, 1, 2012, 7, 10);
			_model = new AgentRestrictionsDetailModel(_period);
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
		}

		[Test]
		public void ShouldLoadDetails()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 6, 28, 2012, 7, 4);

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).Return(_scheduleDayPro).Repeat.AtLeastOnce().IgnoreArguments();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
			}

			using(_mocks.Playback())
			{
				_model.LoadDetails(_scheduleMatrixPro);		
			}

			Assert.AreEqual(28, _model.DetailData().Count());
			Assert.AreEqual(new DateOnly(2012, 6, 18), _model.DetailData()[0].TheDate);
			Assert.AreEqual(new DateOnly(2012, 7, 15),_model.DetailData()[27].TheDate);
			Assert.AreEqual(_scheduleDay, _model.DetailData()[0].SchedulePart);
			Assert.AreEqual(_scheduleDay, _model.DetailData()[27].SchedulePart);

			for (var i = 0; i < 28; i++ )
			{
				Assert.AreEqual(dateOnlyPeriod.StartDate, _model.DetailData()[10].TheDate);
				Assert.AreEqual(dateOnlyPeriod.EndDate, _model.DetailData()[16].TheDate);

				if (i < 10 || i > 16) Assert.IsFalse(_model.DetailData()[i].Enabled);
				else Assert.IsTrue(_model.DetailData()[i].Enabled);
			}
		}
		
		[Test]
		public void ShouldGetDetailDays()
		{
			var startDate = new DateTime(2012, 6, 28, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2012, 7, 4, 0, 0, 0, DateTimeKind.Utc);
			var detailDates = _model.DetailDates(startDate, endDate);
			Assert.AreEqual(28, detailDates.Count);
			Assert.AreEqual(new DateOnly(2012, 6, 18), detailDates.Min());
			Assert.AreEqual(new DateOnly(2012, 7, 15), detailDates.Max());
		}
	}
}
