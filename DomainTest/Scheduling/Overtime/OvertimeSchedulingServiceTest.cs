using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeSchedulingServiceTest
	{
		private IOvertimeSchedulingService _target;
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IOvertimeLengthDecider _overtimeLengthDecider;
		private DateTime _date;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_overtimeLengthDecider = _mocks.StrictMock<IOvertimeLengthDecider>();
			_date = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
			_target = new OvertimeSchedulingService(_schedulingResultStateHolder, _overtimeLengthDecider);
		}

		[Test]
		public void ShouldExecute()
		{
			var persons = new List<IPerson> {PersonFactory.CreatePerson("bill")};
			_target.Execute(persons, new DateOnly(_date), ActivityFactory.CreateActivity("phone"),
			                new MinMax<TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2)));
		}
	}
}
