using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
#pragma warning disable 0649
	[TestFixture, DomainTest]
	class FixReadModelsEventHandlerTest : ISetup
	{
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScheduleDayReadModelRepository ScheduleDayReadModelRepository;
		public FakeScheduleStorage_DoNotUse ScheduleStorage;
		public ReadModelScheduleDayValidator ReadModelScheduleDayValidator;
		public FixReadModelsEventHandler FixHandler;
		public IReadModelValidationResultPersister ValidationResultPersister;
		public FakePersonScheduleDayReadModelPersister PersonScheduleDayReadModelPersister;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator>().For<IReadModelValidator>();
			system.UseTestDouble<ReadModelPersonScheduleDayValidator>().For<IReadModelPersonScheduleDayValidator>();
			system.UseTestDouble<ReadModelScheduleProjectionReadOnlyValidator>().For<IReadModelScheduleProjectionReadOnlyValidator>();
			system.UseTestDouble<ReadModelScheduleDayValidator>().For<IReadModelScheduleDayValidator>();
			system.UseTestDouble<FixReadModelsEventHandler>().For< IHandleEvent<FixReadModelsEvent>>();
			system.UseTestDouble<ReadModelFixer>().For<IReadModelFixer>();

			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage_DoNotUse>().For<IScheduleStorage>();
			system.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakeScheduleDayReadModelRepository>().For<IScheduleDayReadModelRepository>();
			system.UseTestDouble<FakePersonScheduleDayReadModelPersister>().For<IPersonScheduleDayReadModelPersister>();
			system.UseTestDouble<FakeReadModelValidationResultPersistor>().For<IReadModelValidationResultPersister>();
		}


		[Test]
		public void ShouldRemoveScheduleDayReadModelWhenNoPersonPeriodIsFound()
		{
			var scenario = CurrentScenario.Current();
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");

			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016,1,1,8,2016,1,1,17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);

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

			var readModels = ScheduleDayReadModelRepository.ReadModelsOnPerson(new DateOnly(2016, 1, 1), new DateOnly(2016, 1, 1), person.Id.Value);
			readModels.Should().Be.Empty();

		}

		[Test]
		public void ShouldRemovePersonScheduleDayReadModelWhenNoPersonPeriodIsFound()
		{
			var scenario = CurrentScenario.Current();
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");

			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016,1,1,8,2016,1,1,17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);

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
