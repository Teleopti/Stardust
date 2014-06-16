using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class GroupScheduleViewModelFactoryTest
	{
		private DateTime _scheduleDate;

		[SetUp]
		public void Setup()
		{
			_scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0, DateTimeKind.Utc);
		}

		private static string MakeJsonModel(SimpleLayer layer)
		{
			return JsonConvert.SerializeObject(
				new Model
				{
					Shift = new Shift
					{
						Projection = new[]
									{
										layer
									}
					}
				});
		}

		[Test]
		public void ShouldGetSchedulesForGroup()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(1)));
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var readModels = new[] { new PersonScheduleDayReadModel { PersonId = person.Id.Value } };
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			personScheduleDayReadModelRepository.Stub(x => x.ForPeople(period, new[] { person.Id.Value })).Return(readModels);
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }));

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value.ToString());
		}

		[Test]
		public void ShouldGreyConfidentialAbsenceIfNoPermission()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(1)));
			var readModels = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Model = MakeJsonModel(new SimpleLayer {IsAbsenceConfidential = true})
						}
				};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			personScheduleDayReadModelRepository.Stub(x => x.ForPeople(new DateTimePeriod(), new[] { person.Id.Value })).IgnoreArguments().Return(readModels);
			var schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				.Return(new IPerson[] { });
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider);

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);
			result.Single().Projection.Single().Description.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			result.Single().Projection.Single().Color.Should().Be.EqualTo(ConfidentialPayloadValues.DisplayColorHex);
		}

		[Test]
		public void ShouldNotGreyNormalAbsenceIfNoPermission()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(1)));
			var shifts = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Model = MakeJsonModel(new SimpleLayer
								{
									Description = "Vacation",
									Color = "Red",
									IsAbsenceConfidential = false
								})
						}
				};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			personScheduleDayReadModelRepository.Stub(x => x.ForPeople(new DateTimePeriod(), new[] { person.Id.Value })).IgnoreArguments().Return(shifts);
			var schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new List<IPerson>());
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider);

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);

			result.Single().Projection.Single().Description.Should().Be.EqualTo("Vacation");
			result.Single().Projection.Single().Color.Should().Be.EqualTo("Red");
		}


		[Test]
		public void ShouldNotGreyConfidentialAbsenceIfHasPermission()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(1)));
			var shifts = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Model = MakeJsonModel(new SimpleLayer
								{
									Description = "Vacation",
									Color = "Red",
									IsAbsenceConfidential = true
								})
						}
				};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			personScheduleDayReadModelRepository.Stub(x => x.ForPeople(new DateTimePeriod(), new[] { person.Id.Value })).IgnoreArguments().Return(shifts);
			var schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForGroup(new DateOnly(_scheduleDate), Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new[] { person });
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider);

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);

			result.Single().Projection.Single().Description.Should().Be.EqualTo("Vacation");
			result.Single().Projection.Single().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(Color.Red));
		}
	}
}