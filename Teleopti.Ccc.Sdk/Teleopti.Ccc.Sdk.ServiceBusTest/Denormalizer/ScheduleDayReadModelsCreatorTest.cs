using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class ScheduleDayReadModelsCreatorTest
	{
		private MockRepository _mocks;
		private ScheduleDayReadModelsCreator _target;
		private IScheduleRepository _scheduleRep;
		private IScheduleDayReadModelCreator _readModelCreator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleRep = _mocks.DynamicMock<IScheduleRepository>();
			_readModelCreator = _mocks.DynamicMock<IScheduleDayReadModelCreator>();

			_target = new ScheduleDayReadModelsCreator(_scheduleRep, _readModelCreator);
		}

		[Test]
		public void ShouldGetSchedulesAndCallCreator()
		{
			var scenario = _mocks.DynamicMock<IScenario>();
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("he", "j");
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var scheduleRange = _mocks.DynamicMock<IScheduleRange>();
			var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
			                                               new Dictionary<IPerson, IScheduleRange> {{person, scheduleRange}});
			var day = _mocks.DynamicMock<IScheduleDay>();
			var days = new List<IScheduleDay> {day};
			var readModel = new ScheduleDayReadModel{};
			Expect.Call(_scheduleRep.FindSchedulesOnlyInGivenPeriod(null, null, period, scenario)).IgnoreArguments().
					Return(dictionary);
			Expect.Call(scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(days);
			Expect.Call(day.IsScheduled()).Return(true);
			Expect.Call(_readModelCreator.TurnScheduleToModel(day)).Return(readModel);
			_mocks.ReplayAll();
			var ret =_target.GetReadModels(scenario, period, person);
			Assert.That(ret.Contains(readModel), Is.True);
			_mocks.VerifyAll();
		}
	}


}