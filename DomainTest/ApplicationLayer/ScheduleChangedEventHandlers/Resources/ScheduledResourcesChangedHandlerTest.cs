using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	[TestFixture]
	public class ScheduledResourcesChangedHandlerTest
	{
		private ScheduledResourcesChangedHandler _target;
		private IScheduledResourcesReadModelPersister _storage;
		private IPersonRepository _personRepository;
		private ISkillRepository _skillRepository;
		private IScheduleProjectionReadOnlyRepository _scheduleProjectionRepository;
		private IPublishEventsFromEventHandlers _bus;
		private ControllableEventSyncronization _eventSyncronization;
		private IMessageBrokerSender _messageBroker;
		private ISkill _skill;
		private IPerson _person;
		
		private readonly Guid _personId = Guid.NewGuid();
		private readonly Guid _scenarioId = Guid.NewGuid();
		private readonly DateTimePeriod _period = new DateTimePeriod(new DateTime(2013, 8, 16, 12, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 16, 12, 15, 0, DateTimeKind.Utc));
		private readonly DateOnly _date = new DateOnly(2013, 08, 16);

		[SetUp]
		public void Setup()
		{
			_storage = MockRepository.GenerateMock<IScheduledResourcesReadModelPersister>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			_scheduleProjectionRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			_eventSyncronization = new ControllableEventSyncronization();
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerSender>();

			_skill = SkillFactory.CreateSkill("Phone");
			_skill.Activity.SetId(Guid.NewGuid());
			_skill.SetId(Guid.NewGuid());
			_person = PersonFactory.CreatePersonWithPersonPeriod(_date, new[] {_skill});

			_target = new ScheduledResourcesChangedHandler(_personRepository, _skillRepository, _scheduleProjectionRepository, new ScheduledResourcesReadModelUpdater(_storage, _messageBroker, _eventSyncronization), new PersonSkillProvider(), _bus);
		}

		[Test]
		public void ShouldSendASingleBrokerMessageWhenDone()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
							 .Return(new ProjectionChangedEventLayer[] { });

			var period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddHours(1));

			var @event = new ProjectionChangedEvent
				{
					BusinessUnitId = Guid.NewGuid(),
					Datasource = "datasource",
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays =
						new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date,
										Shift = new ProjectionChangedEventShift
											{
												Layers = new[]
													{
														new ProjectionChangedEventLayer
															{
																PayloadId = _skill.Activity.Id.GetValueOrDefault(),
																StartDateTime = period.StartDateTime,
																EndDateTime = period.EndDateTime,
																RequiresSeat = false
															}
													}
											}
									}
							}
				};

			_target.Handle(@event);

			_messageBroker.AssertWasNotCalled(x => x.Send("datasource", @event.BusinessUnitId, period.StartDateTime, period.EndDateTime, Guid.Empty, Guid.Empty, typeof(IScheduledResourcesReadModel), DomainUpdateType.NotApplicable, null), o => o.Repeat.Times(0));

			_eventSyncronization.RunNow();

			_messageBroker.AssertWasCalled(x => x.Send("datasource", @event.BusinessUnitId, period.StartDateTime, period.EndDateTime, Guid.Empty, Guid.Empty, typeof(IScheduledResourcesReadModel), DomainUpdateType.NotApplicable, null), o => o.Repeat.Once());
		}

		[Test]
		public void ShouldRemovePreviousResource()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_storage.Stub(x => x.RemoveResources(_skill.Activity.Id.GetValueOrDefault(), _skill.Id.ToString(), _period, 1, 1))
			        .Return(153);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
			                             .Return(new[]
				                             {
					                             new ProjectionChangedEventLayer
						                             {
							                             PayloadId = _skill.Activity.Id.GetValueOrDefault(),
							                             StartDateTime = _period.StartDateTime,
							                             EndDateTime = _period.EndDateTime,
							                             RequiresSeat = false
						                             }
				                             });

			_target.Handle(new ProjectionChangedEvent
				{
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays =
						new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date
									}
							}
				});

			_bus.AssertWasCalled(x => x.Publish(new ScheduledResourcesChangedEvent()),o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldRemovePreviousResourceWithEfficiency()
		{
			_person.ChangeSkillProficiency(_skill, new Percent(0.8), _person.Period(_date));
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_storage.Stub(x => x.RemoveResources(_skill.Activity.Id.GetValueOrDefault(), _skill.Id.ToString(), _period, 1, 1))
					.Return(153);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
										 .Return(new[]
				                             {
					                             new ProjectionChangedEventLayer
						                             {
							                             PayloadId = _skill.Activity.Id.GetValueOrDefault(),
							                             StartDateTime = _period.StartDateTime,
							                             EndDateTime = _period.EndDateTime,
							                             RequiresSeat = false
						                             }
				                             });

			_target.Handle(new ProjectionChangedEvent
			{
				PersonId = _personId,
				ScenarioId = _scenarioId,
				ScheduleDays =
					new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date
									}
							}
			});

			_bus.AssertWasCalled(x => x.Publish(new ScheduledResourcesChangedEvent()), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldAddNewResourceForInitialLoadOfProjectedSchedule()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_storage.Stub(x => x.AddResources(_skill.Activity.Id.GetValueOrDefault(), false, _skill.Id.ToString(), _period, 1, 1))
					.Return(153);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
										 .Return(new ProjectionChangedEventLayer[] { });

			_target.Handle(new ProjectionChangedEventForScheduleProjection
				{
					IsInitialLoad = true,
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays =
						new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date,
										Shift = new ProjectionChangedEventShift
											{
												Layers = new[]
													{
														new ProjectionChangedEventLayer
															{
																PayloadId = _skill.Activity.Id.GetValueOrDefault(),
																StartDateTime = _period.StartDateTime,
																EndDateTime = _period.EndDateTime,
																RequiresSeat = false
															}
													}
											}
									}
							}
				});

			_bus.AssertWasCalled(x => x.Publish(new ScheduledResourcesChangedEvent()), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldAddNewResource()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_storage.Stub(x => x.AddResources(_skill.Activity.Id.GetValueOrDefault(), false, _skill.Id.ToString(), _period, 1, 1))
					.Return(153);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
										 .Return(new ProjectionChangedEventLayer[]{});

			_target.Handle(new ProjectionChangedEvent
				{
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays = new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date,
										Shift = new ProjectionChangedEventShift
											{
												Layers = new[]
													{
														new ProjectionChangedEventLayer
															{
																PayloadId = _skill.Activity.Id.GetValueOrDefault(),
																StartDateTime = _period.StartDateTime,
																EndDateTime = _period.EndDateTime,
																RequiresSeat = false
															}
													}
											}
									}
							}
				});

			_bus.AssertWasCalled(x => x.Publish(new ScheduledResourcesChangedEvent()), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldExitWhenPersonNotFound()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(null);
			_target.Handle(new ProjectionChangedEvent
				{
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays =
						new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date,
										Shift = new ProjectionChangedEventShift
											{
												Layers = new[]
													{
														new ProjectionChangedEventLayer
															{
																PayloadId = _skill.Activity.Id.GetValueOrDefault(),
																StartDateTime = _period.StartDateTime,
																EndDateTime = _period.EndDateTime,
																RequiresSeat = false
															}
													}
											}
									}
							}
				});

			_bus.AssertWasNotCalled(x => x.Publish(new ScheduledResourcesChangedEvent()), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldAddNewResourceWithEfficiency()
		{
			_personRepository.Stub(x => x.Get(_personId)).Return(_person);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_storage.Stub(x => x.AddResources(_skill.Activity.Id.GetValueOrDefault(), false, _skill.Id.ToString(), _period, 1, 1))
					.Return(153);
			_storage.Stub(x => x.RemoveResources(_skill.Activity.Id.GetValueOrDefault(), _skill.Id.ToString(), _period, 1, 1))
			        .Return(152);
			_scheduleProjectionRepository.Stub(x => x.ForPerson(_date, _personId, _scenarioId))
										 .Return(new ProjectionChangedEventLayer[] { });
			_person.ChangeSkillProficiency(_skill,new Percent(0.8), _person.Period(_date));

			_target.Handle(new ProjectionChangedEvent
				{
					PersonId = _personId,
					ScenarioId = _scenarioId,
					ScheduleDays =
						new[]
							{
								new ProjectionChangedEventScheduleDay
									{
										Date = _date,
										Shift = new ProjectionChangedEventShift
											{
												Layers = new[]
													{
														new ProjectionChangedEventLayer
															{
																PayloadId = _skill.Activity.Id.GetValueOrDefault(),
																StartDateTime = _period.StartDateTime,
																EndDateTime = _period.EndDateTime,
																RequiresSeat = false
															}
													}
											}
									}
							}
				});

			_bus.AssertWasCalled(x => x.Publish(new ScheduledResourcesChangedEvent()), o => o.IgnoreArguments());
		}

	}

}