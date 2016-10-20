using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	public class ScheduleDaySignificantPartTest
	{
		private readonly DateTimePeriod rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);

		[Test]
		public void SignificantPartWithoutMainShiftWithPersonalShift()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var person = PersonFactory.CreatePerson();
			var period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
			var part = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2001, 1, 1));

			var ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(ActivityFactory.CreateActivity("d"), person, period, scenario);
			part.Add(ass);

			Assert.AreEqual(SchedulePartView.PersonalShift, part.SignificantPart());

			ass.SetDayOff(new DayOffTemplate(new Description()));
			Assert.AreEqual(SchedulePartView.DayOff, part.SignificantPart());
		}

		[Test]
		public void SignificantPartCallsService()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var person = PersonFactory.CreatePerson();
			var part = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2001, 1, 1));

			Assert.AreEqual(part.SignificantPart(), SchedulePartView.None);

			var service = MockRepository.GenerateMock<ISignificantPartService>();
			((ExtractedSchedule)part).ServiceForSignificantPart = service;

			service.Stub(x => x.SignificantPart()).Return(SchedulePartView.MainShift);

			Assert.AreEqual(SchedulePartView.MainShift, part.SignificantPart());
		}

		[Test]
		public void SignificantPartEmptyShouldReturnNone()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var part = ExtractedSchedule.CreateScheduleDay(dic,
				parameters.Person, new DateOnly(2000, 1, 1));
			Assert.AreEqual(SchedulePartView.None, part.SignificantPart());
		}

		[Test]
		public void ShouldReturnSignificantPartServiceForDisplay()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var part = ExtractedSchedule.CreateScheduleDay(dic,
				parameters.Person, new DateOnly(2000, 1, 1));
			Assert.That(part.SignificantPartForDisplay(), Is.Not.Null);
		}
	}
}