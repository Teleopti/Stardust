using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class ChangeActivityTypeFormValidatorTest : IIsolateSystem
	{
		public ChangeActivityTypeFormValidator Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository CurrentScenario;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepo;
		public Global.FakePermissionProvider PermissionProvider;

		public void Isolate(IIsolate isolate)
		{

		}

		[Test]
		public void ShouldReturnInvalidInputResultWhenInputWithoutDate()
		{
			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				PersonId = Guid.NewGuid(),
				Layers = new EditingLayerModel[] { }
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForInvalidInput);
		}

		[Test]
		public void ShouldReturnInvalidInputResultWhenInputWithoutPersonId()
		{
			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				Layers = new EditingLayerModel[] { }
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForInvalidInput);
		}

		[Test]
		public void ShouldReturnInvalidInputResultWhenPersonNotExist()
		{
			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2018, 6, 22),
				Layers = new EditingLayerModel[] { }
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForInvalidInput);
		}

		[Test]
		public void ShouldReturnInvalidInputResultWhenInputWithoutLayers()
		{
			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = Guid.NewGuid(),
				Layers = new EditingLayerModel[] { }
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForInvalidInput);
		}


		[Test]
		public void ShouldReturnInvalidInputResultIfAnyLayerActivityNotExist()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{ ActivityId= Guid.NewGuid() }
				}
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.ActivityTypeChangedByOthers);
		}

		[Test]
		public void ShouldReturnErrorWhenChangeActivityTypeCommandIfAnyLayerNotExist()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var activity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(activity);


			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2018, 6, 22));
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(activity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.ShiftLayers.First().SetId(Guid.NewGuid());

			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{
						ActivityId= activity.Id.GetValueOrDefault(),
						ShiftLayerIds = new[]{ Guid.NewGuid()}
					}
				}
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.ShiftChangedByOthers);
		}

		[Test]
		public void ShouldReturnInvalidInputResultWhenAddNewLayerWithoutStartTimeOrEndTime()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var activity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(activity);


			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2018, 6, 22));
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(activity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.ShiftLayers.First().SetId(Guid.NewGuid());

			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{
						ActivityId= activity.Id.GetValueOrDefault(),
						ShiftLayerIds = new[]{ personAss.ShiftLayers.First().Id.Value},
						IsNew = true
					}
				}
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForInvalidInput);
		}
		[Test]
		public void ShouldReturnPermissionErrorResultWhenEditingProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var activity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(activity);


			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2018, 6, 22));
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(activity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.ShiftLayers.First().SetId(Guid.NewGuid());

			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2018, 6, 23);

			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{
						ActivityId= activity.Id.GetValueOrDefault(),
						ShiftLayerIds = new[]{ personAss.ShiftLayers.First().Id.Value}
					}
				}
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForModifyWriteProtectedSchedule);
		}


		[Test]
		public void ShouldReturnPermissionErrorResultWhenNoPermissionToEditUnpublishedSchedule()
		{
			PermissionProvider.Enable();


			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);


			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var activity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(activity);


			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2018, 6, 22));
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(activity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.ShiftLayers.First().SetId(Guid.NewGuid());

			PermissionProvider.PublishToDate(new DateOnly(2018, 6, 21));
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, person, new DateOnly(2018, 6, 22));


			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{
						ActivityId= activity.Id.GetValueOrDefault(),
						ShiftLayerIds = new[]{ personAss.ShiftLayers.First().Id.Value}
					}
				}
			});
			result.IsValid.Should().Be.False();
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.SaveFailedForNoPermissionToEditUnpublishedSchedule);

		}


		[Test]
		public void ShouldReturnResultWithoutAnyError()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);


			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var emailActivity = ActivityFactory.CreateActivity("email", Color.Yellow);
			ActivityRepository.Has(emailActivity);


			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2018, 6, 22));
			PersonAssignmentRepo.Add(personAss);


			personAss.AddActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.ShiftLayers.First().SetId(Guid.NewGuid());

			var result = Target.Validate(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {
					new EditingLayerModel{
						ActivityId= emailActivity.Id.GetValueOrDefault(),
						ShiftLayerIds = new[]{ personAss.ShiftLayers.First().Id.Value}
					}
				}
			});

			result.IsValid.Should().Be.True();
			result.ErrorMessages.Should().Be.Empty();
		}
	}
}
