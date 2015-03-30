using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	[TestFixture]
	public class ScheduledResourcesPersonPeriodAddedHandlerTest
	{
		private ScheduledResourcesPersonPeriodAddedHandler _target;
		private IScheduledResourcesReadModelPersister _storage;
		private readonly Guid _activityId = Guid.NewGuid();
		private readonly Guid _personId = Guid.NewGuid();
		private ISkill _skill;
		private readonly DateOnly _date = new DateOnly(2010,1,1);
		private readonly DateTimePeriod _period = new DateTimePeriod(new DateTime(2010, 1, 1, 12, 0, 0, DateTimeKind.Utc),
		                                                             new DateTime(2010, 1, 1, 12, 15, 0, DateTimeKind.Utc));
		private ISkillRepository _skillRepository;
		private IScheduleProjectionReadOnlyRepository _readModelFinder;
		private IScenarioRepository _scenarioRepository;
		private IScenario _scenario;

		[SetUp]
		public void Setup()
		{
			_storage = MockRepository.GenerateMock<IScheduledResourcesReadModelPersister>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_readModelFinder = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			_skill = SkillFactory.CreateSkill("Skill1");
			_skill.SetId(Guid.NewGuid());
			_scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			_target = new ScheduledResourcesPersonPeriodAddedHandler(new ScheduledResourcesReadModelUpdater(_storage, null, new ControllableEventSyncronization()), _readModelFinder, _skillRepository, _scenarioRepository);
		}

		[Test]
		public void ShouldHandlePersonPeriodWithSkillEfficiencyAdded()
		{
			_readModelFinder.Stub(
				x => x.ForPerson(new DateOnlyPeriod(_date, _date.AddDays(2)), _personId, _scenario.Id.GetValueOrDefault()))
			                .Return(new[]
				                {
					                new ProjectionChangedEventLayer
						                {
							                StartDateTime = _period.StartDateTime,
							                EndDateTime = _period.EndDateTime,
							                PayloadId = _activityId
						                }
				                });

			_readModelFinder.Stub(
				x => x.ForPerson(new DateOnlyPeriod(_date.AddDays(2), _date.AddDays(10)), _personId, _scenario.Id.GetValueOrDefault()))
							.Return(new[]
				                {
					                new ProjectionChangedEventLayer
						                {
							                StartDateTime = _period.StartDateTime.AddHours(48),
							                EndDateTime = _period.EndDateTime.AddHours(48),
							                PayloadId = _activityId
						                }
				                });

			_readModelFinder.Stub(
				x => x.ForPerson(new DateOnlyPeriod(_date, _date.AddDays(10)), _personId, _scenario.Id.GetValueOrDefault()))
							.Return(new[]
				                {
					                new ProjectionChangedEventLayer
						                {
							                StartDateTime = _period.StartDateTime.AddHours(10),
							                EndDateTime = _period.EndDateTime.AddHours(10),
							                PayloadId = _activityId
						                }
				                });

			_storage.Stub(x => x.RemoveResources(_activityId, _skill.Id.ToString(), _period.MovePeriod(TimeSpan.FromHours(10)), 1, 1)).Return(2);
			_storage.Stub(x => x.AddResources(_activityId, false, _skill.Id.ToString(), _period, 1, 1)).Return(3);
			_storage.Stub(x => x.AddResources(_activityId, false, _skill.Id.ToString(), _period.MovePeriod(TimeSpan.FromHours(48)), 1, 1)).Return(4);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);

			_target.Handle(new PersonPeriodAddedEvent
				{
					PersonId = _personId,
					PersonPeriodsBefore = new[]{
							new PersonPeriodDetail
								{
									StartDate = _date.Date,
									EndDate = _date.Date.AddDays(10),
									PersonSkillDetails =
										new[] {new PersonSkillDetail {Active = true, Proficiency = 0.95, SkillId = _skill.Id.GetValueOrDefault()}}
								}},
					PersonPeriodsAfter = new[]
						{
							new PersonPeriodDetail
								{
									StartDate = _date.Date,
									EndDate = _date.Date.AddDays(2),
									PersonSkillDetails =
										new[] {new PersonSkillDetail {Active = true, Proficiency = 0.95, SkillId = _skill.Id.GetValueOrDefault()}}
								},
							new PersonPeriodDetail
								{
									StartDate = _date.Date.AddDays(2),
									EndDate = _date.Date.AddDays(10),
									PersonSkillDetails =
										new[] {new PersonSkillDetail {Active = true, Proficiency = 0.9, SkillId = _skill.Id.GetValueOrDefault()}}
								}
						}
				});

			_storage.AssertWasCalled(x => x.RemoveSkillEfficiency(2,_skill.Id.GetValueOrDefault(),0.95d));
			_storage.AssertWasCalled(x => x.AddSkillEfficiency(3,_skill.Id.GetValueOrDefault(),0.95d));
			_storage.AssertWasCalled(x => x.AddSkillEfficiency(4,_skill.Id.GetValueOrDefault(),0.9d));
		}
	}
}
