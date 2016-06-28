using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ReadModelValidator
{
	[TestFixture, DomainTest]
	public class ReadModelValidatorTest : ISetup
	{
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScheduleStorage ScheduleStorage;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<Domain.ApplicationLayer.ReadModelValidator.ReadModelValidator>().For<IReadModelValidator>();
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
		}

		[Test]
		public void ShouldFindErrorInScheduleProjectionReadOnlyWhenProjectionLayerMismatchWithScheduleData()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016, 1, 1, 8, 2016, 1, 1,  17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);
			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-01-01".Date(),
					ScenarioId = scenario.Id.Value,
					PersonId = person.Id.Value,
					StartDateTime = "2016-01-01 8:00".Utc(),
					EndDateTime = "2016-01-01 15:00".Utc()
				});

			var result = new List<ReadModelValidationResult>();
			Action<ReadModelValidationResult> action = x =>
			{
				result.Add(x);
			};

			Target.SetTargetTypes(new List<ValidateReadModelType> {ValidateReadModelType.ScheduleProjectionReadOnly});
			Target.Validate(new DateTime(2016, 1, 1), new DateTime(2016, 1, 1), action, true);
	
			result.Count().Should().Be.EqualTo(1);
			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);
			result.Single().Date.Should().Be.EqualTo("2016-01-01".Date().Date);
			result.Single().Type.Should().Be.EqualTo(ValidateReadModelType.ScheduleProjectionReadOnly);
		}

		[Test]
		public void ShouldFindNoRecordWhenProjectionLayerMatchWithScheduleData()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(new DateTime(2016, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);
			personAssignment.ShiftLayers.Single().Payload.WithId();
			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-01-01".Date(),
					ScenarioId = scenario.Id.Value,
					PersonId = person.Id.Value,
					StartDateTime = "2016-01-01 8:00".Utc(),
					EndDateTime = "2016-01-01 17:00".Utc(),
					PayloadId = personAssignment.ShiftLayers.Single().Payload.Id.Value,
					ContractTime = new TimeSpan(9, 0, 0),
					Name = personAssignment.ShiftLayers.Single().Payload.Name,
					ShortName = ""
				});

			var result = new List<ReadModelValidationResult>();
			Action<ReadModelValidationResult> action = x =>
			{
				result.Add(x);
			};
			Target.Validate(new DateTime(2016, 1, 1), new DateTime(2016, 1, 1), action, true);


			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void CanBuildReadModelFromScheduleWithSimpleShift()
		{
			var scenario = CurrentScenario.Current();
			var date = new DateOnly(2016, 01, 01);
			var person = PersonFactory.CreatePersonWithGuid("Peter", "T");
			PersonRepository.Has(person);

			var dateTimePeriod = new DateTimePeriod(2016, 01, 01, 8, 2016, 01, 01, 17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);

			ScheduleStorage.Add(personAssignment);


			var readmodels = Target.BuildReadModel(person, date).ToList();

			readmodels.Count().Should().Be.EqualTo(1);
			var readmodel = readmodels.First();
			readmodel.BelongsToDate.Should().Be.EqualTo(date);
			readmodel.StartDateTime.Should().Be.EqualTo(dateTimePeriod.StartDateTime);
			readmodel.EndDateTime.Should().Be.EqualTo(dateTimePeriod.EndDateTime);
			readmodel.PersonId.Should().Be.EqualTo(person.Id.Value);


		}

		[Test]
		public void ShouldFindErrorInPersonScheduleDayReadModel()
		{
			var scenario = CurrentScenario.Current();
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			var personPeriod = new PersonPeriod(new DateOnly(2016, 1, 1),
				PersonContractFactory.CreatePersonContract(ContractFactory.CreateContract("_")), team);
			personPeriod.WithId();
			person.AddPersonPeriod(personPeriod);
			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016, 1, 1, 8, 2016, 1, 1, 17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);

			var result = new List<ReadModelValidationResult>();
			Action<ReadModelValidationResult> action = x =>
			{
				result.Add(x);
			};

			Target.SetTargetTypes(new List<ValidateReadModelType> {ValidateReadModelType.PersonScheduleDay});
			Target.Validate(new DateTime(2016, 1, 1), new DateTime(2016, 1, 1), action, true);

			result.Count().Should().Be.EqualTo(1);
			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);
			result.Single().Date.Should().Be.EqualTo("2016-01-01".Date().Date);
			result.Single().Type.Should().Be.EqualTo(ValidateReadModelType.PersonScheduleDay);
		}

		[Test]
		public void ShouldFindNoErrorInNormalPersonScheduleDayReadModel()
		{
			var scenario = CurrentScenario.Current();
			var site = SiteFactory.CreateSimpleSite("s");
			site.WithId();
			var team = TeamFactory.CreateTeamWithId("t");
			team.Site = site;

			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			var personPeriod = new PersonPeriod(new DateOnly(2016, 1, 1),
				PersonContractFactory.CreatePersonContract(ContractFactory.CreateContract("_")), team);
			personPeriod.WithId();
			person.AddPersonPeriod(personPeriod);
			PersonRepository.Has(person);
			var dateTimePeriod = new DateTimePeriod(2016, 1, 1, 8, 2016, 1, 1, 17);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);
			ScheduleStorage.Add(personAssignment);
			PersonAssignmentRepository.Add(personAssignment);

			var result = new List<ReadModelValidationResult>();
			Action<ReadModelValidationResult> action = x =>
			{
				result.Add(x);
			};

			Target.SetTargetTypes(new List<ValidateReadModelType> { ValidateReadModelType.PersonScheduleDay });
			Target.Validate(new DateTime(2016, 1, 1), new DateTime(2016, 1, 1), action, true);

			result.Count().Should().Be.EqualTo(0);
		}

		
	}
}