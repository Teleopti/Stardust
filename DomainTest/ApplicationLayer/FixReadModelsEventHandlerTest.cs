using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
#pragma warning disable 0649
	[TestFixture, DomainTest]
	class FixReadModelsEventHandlerTest : IIsolateSystem
	{
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeScenarioRepository CurrentScenario;
		public Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScheduleDayReadModelRepository ScheduleDayReadModelRepository;
		public IScheduleStorage ScheduleStorage;
		public FixReadModelsEventHandler FixHandler;
		public IReadModelValidationResultPersister ValidationResultPersister;
		public FakePersonScheduleDayReadModelPersister PersonScheduleDayReadModelPersister;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator>().For<IReadModelValidator>();
			isolate.UseTestDouble<ReadModelPersonScheduleDayValidator>().For<IReadModelPersonScheduleDayValidator>();
			isolate.UseTestDouble<ReadModelScheduleProjectionReadOnlyValidator>().For<IReadModelScheduleProjectionReadOnlyValidator>();
			isolate.UseTestDouble<ReadModelScheduleDayValidator>().For<IReadModelScheduleDayValidator>();
			isolate.UseTestDouble<FixReadModelsEventHandler>().For< IHandleEvent<FixReadModelsEvent>>();
			isolate.UseTestDouble<ReadModelFixer>().For<IReadModelFixer>();

			isolate.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey>>();
			isolate.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakeScheduleDayReadModelRepository>().For<IScheduleDayReadModelRepository>();
			isolate.UseTestDouble<FakePersonScheduleDayReadModelPersister>().For<IPersonScheduleDayReadModelPersister>();
			isolate.UseTestDouble<FakeReadModelValidationResultPersistor>().For<IReadModelValidationResultPersister>();
		}


		[Test]
		public void ShouldRemoveScheduleDayReadModelWhenNoPersonPeriodIsFound()
		{
			var scenario = CurrentScenario.Has("Default");
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");

			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016,1,1,8,2016,1,1,17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);

			ScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				PersonId = person.Id.Value,
				Date = new DateTime(2016,1,1),
				StartDateTime = new DateTime(2016,1,1,8,0,0),
				EndDateTime = new DateTime(2016,1,1,17,0,0),
				Workday = true,
				NotScheduled = false,
				Label = "sd"
			});

			ValidationResultPersister.SaveScheduleDay(new ReadModelValidationResult
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateTime(2016, 1, 1),
				IsValid = false,
				Type = ValidateReadModelType.ScheduleDay
			});

			var fixReadModelEvent = new FixReadModelsEvent
			{
				Targets = ValidateReadModelType.ScheduleDay
			};

			FixHandler.Handle(fixReadModelEvent);

			var readModels = ScheduleDayReadModelRepository.ForPerson(new DateOnly(2016, 1, 1), person.Id.Value);
			readModels.Should().Be.Null();

		}

		[Test]
		public void ShouldRemovePersonScheduleDayReadModelWhenNoPersonPeriodIsFound()
		{
			var scenario = CurrentScenario.Has("Default");
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");

			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016,1,1,8,2016,1,1,17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);

			ValidationResultPersister.SavePersonScheduleDay(new ReadModelValidationResult
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateTime(2016, 1, 1),
				IsValid = false,
				Type = ValidateReadModelType.PersonScheduleDay
			});
			PersonScheduleDayReadModelPersister.Updated = new List<PersonScheduleDayReadModel>
			{
				new PersonScheduleDayReadModel
				{
					PersonId = person.Id.Value,
					Date = new DateTime(2016, 1, 1)
				}
			};

			var fixReadModelEvent = new FixReadModelsEvent
			{
				Targets = ValidateReadModelType.PersonScheduleDay
			};

			FixHandler.Handle(fixReadModelEvent);

			var readModels = PersonScheduleDayReadModelPersister.Updated;
			readModels.Should().Be.Empty();
		}
	}
	
#pragma warning restore 0649
}
