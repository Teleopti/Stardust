using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftEditor;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class MultipleChangeScheduleCommandHandlerTest : IIsolateSystem
	{
		public MultipleChangeScheduleCommandHandler Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeEventPublisher EventPublisher;
		public FakeCurrentDatasource CurrentDatasource;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MutableNow>().For<INow, IMutateNow>();
			isolate.UseTestDouble<MultipleChangeScheduleCommandHandler>().For<IHandleCommand<MultipleChangeScheduleCommand>>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
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
			var person = PersonFactory.CreatePerson("sherklock", "holms").WithId();
			var scenario = ScenarioRepository.Has("default");

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);

			var phonePeriod = new DateTimePeriod(new DateTime(2018, 6, 27, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 16, 0, 0, DateTimeKind.Utc));
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
			var person = PersonFactory.CreatePerson("sherklock", "holms").WithId();
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

		[Test]
		public void ShouldRaiseOnlyOneScheduleChangedEvent()
		{
			var date = new DateTime(2018, 6, 27);
			var person = PersonFactory.CreatePerson("sherklock", "holms").WithId();
			var scenario = ScenarioRepository.Has("default");
			scenario.BusinessUnit.WithId();
			CurrentDatasource.FakeName("datasource");

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			var emailActivity = ActivityFactory.CreateActivity("email", Color.Yellow);

			var phonePeriod = new DateTimePeriod(new DateTime(2018, 6, 27, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 16, 0, 0, DateTimeKind.Utc));
			var emailPeriod = new DateTimePeriod(new DateTime(2018, 6, 27, 9, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 10, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(phoneActivity, phonePeriod, true);
			pa.AddActivity(emailActivity, emailPeriod, true);

			var shiftLayers = pa.ShiftLayers.ToArray();
			shiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var period = new DateTimePeriod(new DateTime(2018, 6, 27, 13, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 27, 14, 0, 0, DateTimeKind.Utc));

			var scheduleDic = ScheduleDictionaryForTest.WithPersonAssignment(scenario, date, pa);

			var trackId = Guid.NewGuid();
			var operatedPersonId = Guid.NewGuid();

			Target.Handle(new MultipleChangeScheduleCommand
			{
				Commands = new List<IScheduleCommand>{
					new ChangeActivityTypeCommand{
						Activity = phoneActivity,
						ShiftLayer = shiftLayers[1]
					},
					new AddActivityCommandSimply {
						Activity = emailActivity,
						Period = period
					},
					new AddPersonalActivityCommandSimply {
						Activity = emailActivity,
						Period = period
					},
					new AddOvertimeActivityCommandSimply {
						Activity = emailActivity,
						Period = period,
						MultiplicatorDefinitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ds", MultiplicatorType.Overtime).WithId()
					}
				},
				ScheduleDictionary = scheduleDic,
				Date = new DateOnly(date),
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = trackId,
					OperatedPersonId = operatedPersonId
				}
			});

			scheduleDic[person].ScheduledDay(new DateOnly(date)).PersonAssignment().HasEvents().Should().Be.False();

			var @event = EventPublisher.PublishedEvents.Single() as ScheduleChangedEvent;
			@event.Date.Should().Be.EqualTo(date);
			@event.PersonId.Should().Be.EqualTo(person.Id.Value);
			@event.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 8, 0, 0));
			@event.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 27, 16, 0, 0));
			@event.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
			@event.CommandId.Should().Be.EqualTo(trackId);
			@event.LogOnBusinessUnitId.Should().Be.EqualTo(scenario.BusinessUnit.Id.GetValueOrDefault());
			@event.InitiatorId.Should().Be.EqualTo(operatedPersonId);
			@event.LogOnDatasource.Should().Be.EqualTo(CurrentDatasource.CurrentName());

		}


	}
}
