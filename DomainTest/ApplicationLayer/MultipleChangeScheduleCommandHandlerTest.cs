using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftEditor;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class MultipleChangeScheduleCommandHandlerTest : IIsolateSystem
	{
		public MultipleChangeScheduleCommandHandler Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MultipleChangeScheduleCommandHandler>().For<IHandleCommand<MultipleChangeScheduleCommand>>();
		}


		[Test]
		public void ShouldChangeActivityType()
		{
			var scenario = ScenarioRepository.Has("default");

			var date = new DateOnly(2018, 6, 25);
			var person = PersonFactory.CreatePerson("Sherk", "Holms").WithId();
			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			var emailActivity = ActivityFactory.CreateActivity("email", Color.Yellow);
			var period = new DateTimePeriod(new DateTime(2018, 6, 25, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 25, 16, 0, 0, DateTimeKind.Utc));

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			pa.AddActivity(phoneActivity, period);

			var scheduleDic = ScheduleDictionaryForTest.WithPersonAssignment(scenario, new DateTime(2018, 6, 25), pa);

			var shiftLayers = pa.ShiftLayers.ToArray();
			shiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var command = new MultipleChangeScheduleCommand
			{
				ScheduleDictionary = scheduleDic,
				Date = date,
				Person = person,
				Commands = new List<IScheduleCommand> {
					new ChangeActivityTypeCommand {
						Activity = emailActivity,
						ShiftLayer = shiftLayers[0]
					}
				}
			};
			Target.Handle(command);

			pa = scheduleDic[person].ScheduledDay(date).PersonAssignment();


			pa.ShiftLayers.Single().Payload.Id.Should().Be.EqualTo(emailActivity.Id.Value);
			pa.ShiftLayers.Single().Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldAddActivity()
		{
			var scenario = ScenarioRepository.Has("default");

			var date = new DateOnly(2018, 6, 25);
			var person = PersonFactory.CreatePerson("Sherk", "Holms").WithId();
			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			var emailActivity = ActivityFactory.CreateActivity("email", Color.Yellow);
			var period = new DateTimePeriod(new DateTime(2018, 6, 25, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 25, 16, 0, 0, DateTimeKind.Utc));

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			pa.AddActivity(phoneActivity, period);

			var scheduleDic = ScheduleDictionaryForTest.WithPersonAssignment(scenario, new DateTime(2018, 6, 25), pa);

			var shiftLayers = pa.ShiftLayers.ToArray();
			shiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var activityPeriod = new DateTimePeriod(new DateTime(2018, 6, 25, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 25, 11, 0, 0, DateTimeKind.Utc));

			var command = new MultipleChangeScheduleCommand
			{
				ScheduleDictionary = scheduleDic,
				Date = date,
				Person = person,
				Commands = new List<IScheduleCommand> {
					new AddActivityCommandSimply {
						Activity = emailActivity,
						Period = activityPeriod
					}
				}
			};

			Target.Handle(command);

			pa = scheduleDic[person].ScheduledDay(date).PersonAssignment();
			pa.ShiftLayers.Count().Should().Be.EqualTo(2);

			pa.ShiftLayers.First().Payload.Id.Should().Be.EqualTo(phoneActivity.Id.Value);
			pa.ShiftLayers.First().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 25, 8, 0, 0));
			pa.ShiftLayers.First().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 25, 16, 0, 0));
			pa.ShiftLayers.First().OrderIndex.Should().Be.EqualTo(0);

			pa.ShiftLayers.Second().Payload.Id.Should().Be.EqualTo(emailActivity.Id.Value);
			pa.ShiftLayers.Second().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 25, 10, 0, 0));
			pa.ShiftLayers.Second().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 25, 11, 0, 0));
			pa.ShiftLayers.Second().OrderIndex.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddPersonalActivity()
		{
			var date = new DateTime(2018, 6, 27);
			var person = PersonFactory.CreatePerson("sherklock", "holms");
			var scenario = ScenarioRepository.Has("default");

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);

			var phonePeriod = new DateTimePeriod(new DateTime(2018, 6, 27, 8, 0, 0,DateTimeKind.Utc), new DateTime(2018, 6, 27, 16, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(phoneActivity, phonePeriod);

			var period = new DateTimePeriod(new DateTime(2018, 6, 27, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 9, 0, 0, DateTimeKind.Utc));

			var scheduleDic = ScheduleDictionaryForTest.WithPersonAssignment(scenario, date, pa);

			var multipleCommand = new MultipleChangeScheduleCommand
			{
				Commands = new List<IScheduleCommand> {
					new AddPersonalActivityCommandSimply{
						Activity = phoneActivity,
						Period = period
					}
				},
				ScheduleDictionary = scheduleDic,
				Date = new DateOnly(date),
				Person = person
			};
			Target.Handle(multipleCommand);

			pa = scheduleDic[person].ScheduledDay(new DateOnly(date)).PersonAssignment();
			pa.ShiftLayers.Count().Should().Be.EqualTo(2);

			pa.ShiftLayers.First().Payload.Id.Should().Be.EqualTo(phoneActivity.Id.Value);
			pa.ShiftLayers.First().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 8, 0, 0));
			pa.ShiftLayers.First().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 16, 0, 0));
			pa.ShiftLayers.First().OrderIndex.Should().Be.EqualTo(0);

			pa.ShiftLayers.Second().Payload.Id.Should().Be.EqualTo(phoneActivity.Id.Value);
			pa.ShiftLayers.Second().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 8, 0, 0));
			pa.ShiftLayers.Second().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 9, 0, 0));
			pa.ShiftLayers.Second().OrderIndex.Should().Be.EqualTo(1);

		}


		[Test]
		public void ShouldAddOvertimeActivity()
		{
			var date = new DateTime(2018, 6, 27);
			var person = PersonFactory.CreatePerson("sherklock", "holms");
			var scenario = ScenarioRepository.Has("default");

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);

			var phonePeriod = new DateTimePeriod(new DateTime(2018, 6, 27, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 16, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(phoneActivity, phonePeriod);

			var period = new DateTimePeriod(new DateTime(2018, 6, 27, 20, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 22, 0, 0, DateTimeKind.Utc));

			var scheduleDic = ScheduleDictionaryForTest.WithPersonAssignment(scenario, date, pa);

			var multipleCommand = new MultipleChangeScheduleCommand
			{
				Commands = new List<IScheduleCommand> {
					new AddOvertimeActivityCommandSimply{
						Activity = phoneActivity,
						Period = period
					}
				},
				ScheduleDictionary = scheduleDic,
				Date = new DateOnly(date),
				Person = person
			};
			Target.Handle(multipleCommand);

			pa = scheduleDic[person].ScheduledDay(new DateOnly(date)).PersonAssignment();
			pa.ShiftLayers.Count().Should().Be.EqualTo(2);

			pa.ShiftLayers.First().GetType().Name.Should().Be.EqualTo(nameof(MainShiftLayer));
			pa.ShiftLayers.First().Payload.Id.Should().Be.EqualTo(phoneActivity.Id.Value);
			pa.ShiftLayers.First().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 8, 0, 0));
			pa.ShiftLayers.First().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 16, 0, 0));
			pa.ShiftLayers.First().OrderIndex.Should().Be.EqualTo(0);

			pa.ShiftLayers.Second().GetType().Name.Should().Be.EqualTo(nameof(OvertimeShiftLayer));
			pa.ShiftLayers.Second().Payload.Id.Should().Be.EqualTo(phoneActivity.Id.Value);
			pa.ShiftLayers.Second().Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 20, 0, 0));
			pa.ShiftLayers.Second().Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 22, 0, 0));
			pa.ShiftLayers.Second().OrderIndex.Should().Be.EqualTo(1);

		}
	}
}
