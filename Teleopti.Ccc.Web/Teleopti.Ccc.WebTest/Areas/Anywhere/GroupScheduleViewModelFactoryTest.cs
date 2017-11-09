using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class GroupScheduleViewModelFactoryTest
	{
		private DateTime _scheduleDate;
		private CommonAgentNameProvider _commonAgentNameProvider;	

		[SetUp]
		public void Setup()
		{
			_scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var globalSettingRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRepository.Stub(x => x.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting()))
				.IgnoreArguments()
				.Return(new CommonNameDescriptionSetting());
			_commonAgentNameProvider = new CommonAgentNameProvider(globalSettingRepository);
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
			
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository,
				new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }), _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value.ToString());
			result.Single().Name.Should().Be.EqualTo(_commonAgentNameProvider.CommonAgentNameSettings.BuildFor(person));
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
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider, _commonAgentNameProvider);

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
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider, _commonAgentNameProvider);

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
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), new FakeLoggedOnUser(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IPermissionProvider>(), schedulePersonProvider, _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, _scheduleDate);

			result.Single().Projection.Single().Description.Should().Be.EqualTo("Vacation");
			result.Single().Projection.Single().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(Color.Red));
		}

		[Test]
		public void ShouldGetCorrectScheduleTimePeriodOnTheDayWhenDaylightSavingTimeStarts()
		{
			var date = new DateTime(2016, 3, 27);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(date));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();

			var loggedOnUser = new FakeLoggedOnUser();
			var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			loggedOnUser.SetDefaultTimeZone(userTimeZone);

			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), loggedOnUser, personScheduleDayReadModelRepository,
				new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }), _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, date);

			var startTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 3, 27), userTimeZone, TimeZoneInfo.Utc);
			var endTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 3, 28), userTimeZone, TimeZoneInfo.Utc);

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForPeople(
				Arg<DateTimePeriod>.Matches(period => period == new DateTimePeriod(startTime, endTime)),
				Arg<IEnumerable<Guid>>.Is.Anything
			));
		}

		[Test]
		public void ShouldGetCorrectScheduleTimePeriodOnTheDayWhenDaylightSavingTimeStartsMinusUtc()
		{			
			var date = new DateTime(2016, 3, 13);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(date));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();

			var loggedOnUser = new FakeLoggedOnUser();
			var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			loggedOnUser.SetDefaultTimeZone(userTimeZone);
			
			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), loggedOnUser, personScheduleDayReadModelRepository,
				new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }), _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, date);

			var startTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 3, 13), userTimeZone, TimeZoneInfo.Utc);
			var endTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 3, 14), userTimeZone, TimeZoneInfo.Utc);

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForPeople(
				Arg<DateTimePeriod>.Matches(period => period == new DateTimePeriod(startTime, endTime)),
				Arg<IEnumerable<Guid>>.Is.Anything
			));
		}

		[Test]
		public void ShouldGetCorrectScheduleTimePeriodOnTheDayWhenDaylightSavingTimeEnds()
		{
			var date = new DateTime(2016, 10, 30);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(date));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();

			var loggedOnUser = new FakeLoggedOnUser();
			var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			loggedOnUser.SetDefaultTimeZone(userTimeZone);

			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), loggedOnUser, personScheduleDayReadModelRepository,
				new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }), _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, date);

			var startTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 10, 30), userTimeZone, TimeZoneInfo.Utc);
			var endTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 10, 31), userTimeZone, TimeZoneInfo.Utc);

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForPeople(
				Arg<DateTimePeriod>.Matches(period => period == new DateTimePeriod(startTime, endTime)),
				Arg<IEnumerable<Guid>>.Is.Anything
			));
		}

		[Test]
		public void ShouldGetCorrectScheduleTimePeriodOnTheDayWhenDaylightSavingTimeEndsMinusUtc()
		{
			var date = new DateTime(2016, 11, 6);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(date));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();

			var loggedOnUser = new FakeLoggedOnUser();
			var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			loggedOnUser.SetDefaultTimeZone(userTimeZone);

			var target = new GroupScheduleViewModelFactory(new GroupScheduleViewModelMapper(), loggedOnUser, personScheduleDayReadModelRepository,
				new FakePermissionProvider(), new FakeSchedulePersonProvider(new[] { person }), _commonAgentNameProvider);

			var result = target.CreateViewModel(Guid.Empty, date);

			var startTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 11, 6), userTimeZone, TimeZoneInfo.Utc);
			var endTime = TimeZoneInfo.ConvertTime(new DateTime(2016, 11, 7), userTimeZone, TimeZoneInfo.Utc);

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForPeople(
				Arg<DateTimePeriod>.Matches(period => period == new DateTimePeriod(startTime, endTime)),
				Arg<IEnumerable<Guid>>.Is.Anything
			));
		}
	}
}