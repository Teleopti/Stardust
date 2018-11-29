using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

using System.Threading;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleActivityCommandServiceTest
	{
		public TeamScheduleActivityCommandService Target;
		public FakeCommandHandler ActivityCommandHandler;
		public FakePersonRepository PersonRepository;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeScenarioRepository CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeShiftCategoryRepository ShiftCategoryRepository;


		[Test]
		public void ShouldInvokeAddActivityCommandHandleWithPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity, person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity, person2, date);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotInvokeAddActivityCommandHandleWithoutPermission()
		{
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnErrorMessagesIfCultureChanged() {
			var date = new DateOnly(2018, 11, 6);

			PermissionProvider.Enable();
			Thread.CurrentThread.CurrentUICulture = CultureInfoFactory.CreateEnglishCulture();

			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person1);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2018, 11, 6, 8, 0, 0),
				EndTime = new DateTime(2018, 11, 6, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};
			var results = Target.AddActivity(input);
			results.Single().ErrorMessages.Single().Contains("No permission to add activity for agent(s)").Should().Be.True();


			Thread.CurrentThread.CurrentUICulture = CultureInfoFactory.CreateChineseCulture();
			results = Target.AddActivity(input);
			results.Single().ErrorMessages.Single().Contains("没有权限为座席代表添加活动").Should().Be.True();
		}

		[Test]
		public void ShouldNotInvokeAddActivityCommandHandleWhenScheduleIsUnpublishedAndNoPermissionToViewUnpublishedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity, person, date);
			PermissionProvider.PublishToDate(date.AddDays(-1));

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			var results = Target.AddActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}

		[Test]
		public void ShouldInvokeAddPersonalActivityCommandHandleWithPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, person2, date);

			var input = new AddPersonalActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddPersonalActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotInvokeAddPersonalActivityCommandHandleWithoutPermission()
		{
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			var input = new AddPersonalActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddPersonalActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotInvokeAddPersonalActivityCommandHandleWhenScheduleIsUnpublishedAndNoPermissionToViewUnpublishedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, person, date);
			PermissionProvider.PublishToDate(date.AddDays(-1));

			var input = new AddPersonalActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			var results = Target.AddPersonalActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}

		[Test]
		public void ShouldInvokeAddOvertimeActivityCommandHandlerWithPermission()
		{
			var date = new DateOnly(2016, 4, 16);
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity, person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity, person2, date);

			var input = new AddOvertimeActivityForm
			{
				StartDateTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndDateTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddOvertimeActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotInvokeAddOvertimeActivityCommandHandlerWithoutPermission()
		{
			PermissionProvider.Enable();
			var date = new DateOnly(2016, 4, 16);
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var input = new AddOvertimeActivityForm
			{
				StartDateTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndDateTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person1.Id.Value,
						Date = date
					},
					new PersonDate
					{
						PersonId = person2.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddOvertimeActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotInvokeAddOvertimeActivityCommandHandleWhenScheduleIsUnpublishedAndNoPermissionToViewUnpublishedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity, person, date);
			PermissionProvider.PublishToDate(date.AddDays(-1));

			var input = new AddOvertimeActivityForm
			{
				ActivityId = Guid.NewGuid(),
				StartDateTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndDateTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			var results = Target.AddOvertimeActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}

		[Test]
		public void ShouldNotRemoveActivityWithoutPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);
			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityInfo>
				{
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							},
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					},
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					}
				}
			};

			ActivityCommandHandler.ResetCalledCount();
			Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldRemoveActivityWithPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.RemoveActivity, person, date);

			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityInfo>
				{
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							},
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					},
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					}
				}
			};

			ActivityCommandHandler.ResetCalledCount();
			Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnErrorMessageWhenMoveActivityIfInputWithMulitipleShiftLayers()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 7, 24);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2018, 7, 24, 8, 2018, 7, 24, 16));

			var emailActivity = ActivityFactory.CreateActivity("email", Color.Red).WithId();
			personAss.AddActivity(emailActivity, new DateTimePeriod(new DateTime(2018, 7, 24, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 7, 24, 9, 0, 0, DateTimeKind.Utc)));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> { personAss.ShiftLayers.FirstOrDefault().Id.Value , personAss.ShiftLayers.Last().Id.Value }
					}
				},

				StartTime = new DateTime(2018, 7, 24, 10, 0, 0)
			};
			var result = Target.MoveActivity(input);
			result.First().ErrorMessages.Contains(Resources.CanNotMoveMultipleActivitiesForSelectedAgents).Should().Be.True();
		}
		[Test]
		public void ShouldNotMoveActivityWhenNoMoveActivityPermission()
		{
			var scenario = CurrentScenario.Has("Default");
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> { personAss.ShiftLayers.FirstOrDefault().Id.Value }
					}
				},

				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(1);
			result.First().ErrorMessages.Contains(Resources.NoPermissionMoveAgentActivity).Should().Be.True();
		}

		[Test]
		public void ShouldNotMoveOvertimeWhenNoMoveOvertimePermission()
		{
			var scenario = CurrentScenario.Has("Default");
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> { personAss.ShiftLayers.FirstOrDefault(sl=>sl is OvertimeShiftLayer).Id.Value }
					}
				},

				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(1);
			result.First().ErrorMessages.Contains(Resources.NoPermissionMoveAgentOvertime).Should().Be.True();
		}

		[Test]
		public void ShouldReturnWriteProtectedMsgWhenWriteProtected()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MoveActivity);
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			person.PersonWriteProtection.PersonWriteProtectedDate = date;

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},

				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(1);
			result.First().ErrorMessages.Contains(Resources.WriteProtectSchedule).Should().Be.True();
		}

		[Test]
		public void ShouldMoveActivityIfInputWithSingleShiftLayer()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.AddActivity(personAss.ShiftLayers.First().Payload, new DateTimePeriod(2016, 4, 16, 7, 2016, 4, 16, 10));
			personAss.AddActivity(personAss.ShiftLayers.First().Payload, new DateTimePeriod(2016, 4, 16, 9, 2016, 4, 16, 10));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local));
		}

		[Test]
		public void ShouldNotRemoveWriteProtectedActivity()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var date = new DateOnly(2016, 4, 16);
			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityInfo>
				{
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					}
				}
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Single().ErrorMessages.Count().Should().Be.EqualTo(2);
			results.Single().ErrorMessages.Should().Contain(Resources.NoPermissionRemoveAgentActivity);
			results.Single().ErrorMessages.Should().Contain(Resources.WriteProtectSchedule);
		}

		[Test]
		public void ShouldNotAddActivityToWriteProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var date = new DateOnly(2016, 4, 16);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonDates = new[]
				{
					new PersonDate
					{
						PersonId = person.Id.Value,
						Date = date
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.AddActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages.Count.Should().Be.EqualTo(2);
			results.First().ErrorMessages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);
			results.First().ErrorMessages[1].Should().Be.EqualTo(Resources.NoPermissionAddAgentActivity);
		}

		[Test]
		public void ShouldCovertNewStartToUTC()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local));

		}

		[Test]
		public void ShouldInvokeMoveShiftLayerCommandWithPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(input.StartTime);

		}
		[Test]
		public void ShouldNotInvokeMoveShiftLayerCommandWhenMoveToTimeMakeShiftLengthExceed36Hours()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.AddActivity(ActivityFactory.CreateActivity("ac"), new DateTimePeriod(2016, 4, 16, 2, 2016, 4, 17, 13));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,

						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				StartTime = new DateTime(2016, 4, 17, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.First().ErrorMessages.Contains(Resources.ShiftLengthExceed36Hours).Should().Be.True();
		}

		[Test]
		public void ShouldInvokeMoveShiftLayerCommandWhenMoveToTimeMakeShiftLengthIs36Hours()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.AddActivity(ActivityFactory.CreateActivity("ac"), new DateTimePeriod(2016, 4, 16, 2, 2016, 4, 17, 13));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						Date = date,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				StartTime = new DateTime(2016, 4, 17, 6, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
		}

		

		[Test]
		public void ShouldNotAllowRemovingOvertimeWithoutPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.RemoveActivity, person, date);

			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityInfo>
				{
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							},
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
							}
						}
					},
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
								IsOvertime = true
							}
						}
					}
				}
			};

			ActivityCommandHandler.ResetCalledCount();
			var result = Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
			result.Count.Should().Be.EqualTo(1);
			result[0].ErrorMessages[0].Should().Be.EqualTo(Resources.NoPermissionRemoveOvertimeActivity);
		}

		[Test]
		public void ShouldAllowRemovingOvertimeWithOnlyRemoveOvertimePermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.RemoveOvertime, person, date);

			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityInfo>
				{
					new PersonActivityInfo
					{
						PersonId = person.Id.Value,
						ShiftLayers = new List<ShiftLayerDate>
						{
							new ShiftLayerDate
							{
								ShiftLayerId = Guid.NewGuid(),
								Date = date,
								IsOvertime = true
							}
						}
					}
				}
			};

			ActivityCommandHandler.ResetCalledCount();
			var result = Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInvokeMoveShiftCommandWhenHasPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveShiftForm()
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonIds = new[] { person.Id.Value },
				Date = date,
				NewShiftStart = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveShift(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftCommand)ActivityCommandHandler.CalledCommands.First()).NewStartTimeInUtc.Should().Be(input.NewShiftStart);
		}

		[Test]
		public void ShouldNotInvokeMoveShiftCommandWhenHasNoPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);

			var input = new MoveShiftForm()
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonIds = new[] { person.Id.Value },
				Date = date,
				NewShiftStart = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveShift(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}
	}
}
