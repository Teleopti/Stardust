using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	[TestFixture]
	public class ScheduledResourcesPersonSkillAddedHandlerTest
	{
		private ScheduledResourcesPersonSkillAddedHandler _target;
		private IScheduledResourcesReadModelPersister _storage;
		private readonly Guid _activityId = Guid.NewGuid();
		private readonly Guid _personId = Guid.NewGuid();
		private IPersonRepository _personRepository;
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
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_readModelFinder = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			_skill = SkillFactory.CreateSkill("Skill1");
			_skill.SetId(Guid.NewGuid());
			_scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			_target = new ScheduledResourcesPersonSkillAddedHandler(new ScheduledResourcesReadModelUpdater(_storage, null, new ControllableEventSyncronization()), _readModelFinder, _skillRepository, _scenarioRepository);
		}

		[Test]
		public void ShouldHandleNewSkillWithProficiencyWhenNoPreviousPersonSkill()
		{
			_readModelFinder.Stub(
				x => x.ForPerson(new DateOnlyPeriod(_date, _date), _personId, _scenario.Id.GetValueOrDefault()))
			                .Return(new[]
				                {
					                new ProjectionChangedEventLayer
						                {
							                StartDateTime = _period.StartDateTime,
							                EndDateTime = _period.EndDateTime,
							                PayloadId = _activityId
						                }
				                });
			_storage.Stub(x => x.AddResources(_activityId, false, _skill.Id.ToString(), _period, 1, 1)).Return(2);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);

			_target.Handle(new PersonSkillAddedEvent
				{
					PersonId = _personId,
					SkillId = _skill.Id.GetValueOrDefault(),
					StartDate = _date.Date,
					EndDate = _date.Date,
					SkillActive = true,
					Proficiency = 0.9d,
					SkillsBefore = new PersonSkillDetail[] {}
				});

			_storage.AssertWasCalled(x => x.AddSkillEfficiency(2,_skill.Id.GetValueOrDefault(),0.9d));
		}

		[Test]
		public void ShouldHandleNewSkillWithProficiencyWhenPreviousPersonSkillExists()
		{
			var newSkillId = Guid.NewGuid();

			_readModelFinder.Stub(
				x => x.ForPerson(new DateOnlyPeriod(_date, _date), _personId, _scenario.Id.GetValueOrDefault()))
							.Return(new[]
				                {
					                new ProjectionChangedEventLayer
						                {
							                StartDateTime = _period.StartDateTime,
							                EndDateTime = _period.EndDateTime,
							                PayloadId = _activityId
						                }
				                });

			_storage.Stub(x => x.AddResources(_activityId, false, SkillCombination.ToKey(new[]{newSkillId, _skill.Id.GetValueOrDefault()}), _period, 1, 1)).Return(4);
			_storage.Stub(x => x.RemoveResources(_activityId, _skill.Id.ToString(), _period, 1, 1)).Return(2);
			_skillRepository.Stub(x => x.MinimumResolution()).Return(15);
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);

			_target.Handle(new PersonSkillAddedEvent
			{
				PersonId = _personId,
				SkillId = newSkillId,
				StartDate = _date.Date,
				EndDate = _date.Date,
				SkillActive = true,
				Proficiency = 0.8d,
				SkillsBefore = new[] { new PersonSkillDetail { Active = true, Proficiency = 0.9, SkillId = _skill.Id.GetValueOrDefault() } }
			});

			_storage.AssertWasCalled(x => x.RemoveSkillEfficiency(2, _skill.Id.GetValueOrDefault(), 0.9d));
			_storage.AssertWasCalled(x => x.AddSkillEfficiency(4, _skill.Id.GetValueOrDefault(), 0.9d));
			_storage.AssertWasCalled(x => x.AddSkillEfficiency(4, newSkillId, 0.8d));
		}
	}
}
