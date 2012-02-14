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
	public class UpdateScheduleProjectionReadModelTest
	{
		private UpdateScheduleProjectionReadModel target;
		private MockRepository mocks;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private IScheduleRepository scheduleRepository;
		private IPerson person;
		private IScenario scenario;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleProjectionReadOnlyRepository = mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();
			scheduleRepository = mocks.DynamicMock<IScheduleRepository>();
			scenario = ScenarioFactory.CreateScenarioAggregate();
			person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			target = new UpdateScheduleProjectionReadModel(scheduleProjectionReadOnlyRepository, scheduleRepository);
		}

		[Test]
		public void ShouldUpdateScheduleProjectionReadModel()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var period = dateOnlyPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var scheduleDay =
				new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("testar")).
					CreatePartWithMainShift();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
			                                               new Dictionary<IPerson, IScheduleRange> {{person, scheduleRange}});

			using (mocks.Record())
			{
				Expect.Call(()=>scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(dateOnlyPeriod,scenario,person.Id.GetValueOrDefault()));
				Expect.Call(
					() =>
					scheduleProjectionReadOnlyRepository.AddProjectedLayer(dateOnlyPeriod.StartDate, scenario,
					                                                       person.Id.GetValueOrDefault(), null)).IgnoreArguments();
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, period, scenario)).IgnoreArguments().
					Return(dictionary);
				Expect.Call(scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new[] {scheduleDay});
				Expect.Call(scheduleRange.TotalPeriod()).Return(period);
			}
			using (mocks.Playback())
			{
				target.Execute(scenario, period, person);
			}
		}

		[Test]
		public void ShouldUpdateScheduleProjectionReadModelButNotDeleteWhenOptionIsSet()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var period = dateOnlyPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var scheduleDay =
				new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("testar")).
					CreatePartWithMainShift();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
														   new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } });

			using (mocks.Record())
			{
				Expect.Call(() => scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(dateOnlyPeriod, scenario, person.Id.GetValueOrDefault())).Repeat.Never();
				Expect.Call(
					() =>
					scheduleProjectionReadOnlyRepository.AddProjectedLayer(dateOnlyPeriod.StartDate, scenario,
																		   person.Id.GetValueOrDefault(), null)).IgnoreArguments();
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, period, scenario)).IgnoreArguments().
					Return(dictionary);
				Expect.Call(scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new[] { scheduleDay });
				Expect.Call(scheduleRange.TotalPeriod()).Return(period);
			}
			using (mocks.Playback())
			{
				target.SetSkipDelete(true);
				target.Execute(scenario, period, person);
			}
		}
	}
}
