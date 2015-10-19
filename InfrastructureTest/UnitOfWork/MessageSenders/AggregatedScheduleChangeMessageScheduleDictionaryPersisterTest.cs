using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	[PrincipalAndStateTest]
	[Ignore]
	public class AggregatedScheduleChangeMessageScheduleDictionaryPersisterTest
	{
		public FakeMessageSender MessageSender;
		public IScheduleDictionaryPersister Target;
		public IJsonDeserializer Deserializer;
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		
		[Test]
		public void ShouldSendOneAggregatedScheduleChangeMessageForTheWholeDictionary()
		{

			//var scenario = new Scenario(".");
			//var schedules = new ScheduleDictionaryForTest(scenario, "2015-10-19".Utc());
			//var person1 = new Person();
			//person1.SetId(Guid.NewGuid());
			//var person2 = new Person();
			//schedules.AddPersonAssignment(new PersonAssignment(person1, scenario, "2015-10-19".Date()));
			//person2.SetId(Guid.NewGuid());
			//schedules.AddPersonAssignment(new PersonAssignment(person1, scenario, "2015-10-19".Date()));



			//var scenario = new Scenario(".");
			//var period = new DateTimePeriod("2015-10-19".Utc(), "2015-10-20".Utc());
			////var schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(period));
			//var schedules = new ScheduleDictionaryForTest(scenario, period);

			//var person1 = new Person();
			//person1.SetId(Guid.NewGuid());
			//var range1 = new ScheduleRange(schedules, new ScheduleParameters(scenario, person1, period));
			//range1.Add(new PersonAssignment(person1, scenario, "2015-10-19".Date()));
			//schedules.AddTestItem(person1, range1);

			//var person2 = new Person();
			//person2.SetId(Guid.NewGuid());
			//var range2 = new ScheduleRange(schedules, new ScheduleParameters(scenario, person2, period));
			//range2.Add(new PersonAssignment(person2, scenario, "2015-10-19".Date()));
			//schedules.AddTestItem(person2, range2);



			var person1 = PersonFactory.CreatePerson("persist", "test1");
			var person2 = PersonFactory.CreatePerson("persist", "test2");
			var Activity = new Activity("persist test");
			var ShiftCategory = new ShiftCategory("persist test");
			var Scenario = new Scenario("scenario");
			var Absence = new Absence { Description = new Description("perist", "test") };
			var DefinitionSet = new MultiplicatorDefinitionSet("persist test", MultiplicatorType.Overtime);
			var DayOffTemplate = new DayOffTemplate(new Description("persist test"));
			using (var unitOfWork = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				new PersonRepository(new ThisUnitOfWork(unitOfWork)).Add(person1);
				new PersonRepository(new ThisUnitOfWork(unitOfWork)).Add(person2);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new MultiplicatorDefinitionSetRepository(unitOfWork).Add(DefinitionSet);
				new DayOffTemplateRepository(unitOfWork).Add(DayOffTemplate);
				unitOfWork.PersistAll();
			}


			IScheduleDictionary schedules;
			using (var unitOfWork = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var rep = new ScheduleRepository(unitOfWork);
				schedules = rep.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
					Scenario,
					new PersonProvider(new[] { person1 }),
					new ScheduleDictionaryLoadOptions(true, true),
					new List<IPerson> { person1 });
			}

			var scheduleDay = schedules[person1].ScheduledDay("2015-10-19".Date());
			scheduleDay.CreateAndAddActivity(Activity, new DateTimePeriod("2015-10-19 08:00".Utc(), "2015-10-19 17:00".Utc()), null);
			schedules.Modify(ScheduleModifier.Scheduler,
				scheduleDay,
				NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(),
				new FakeScheduleTagSetter());

			var scheduleDay2 = schedules[person2].ScheduledDay("2015-10-19".Date());
			scheduleDay2.CreateAndAddActivity(Activity, new DateTimePeriod("2015-10-19 08:00".Utc(), "2015-10-19 17:00".Utc()), null);
			schedules.Modify(ScheduleModifier.Scheduler,
				scheduleDay2,
				NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(),
				new FakeScheduleTagSetter());

			Target.Persist(schedules);

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			Deserializer.DeserializeObject<Guid[]>(message.BinaryData).Should().Have.SameValuesAs(new[] { person1.Id.Value, person2.Id.Value });
		}

	}
}