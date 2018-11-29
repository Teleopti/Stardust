using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	public class MyTimeWebRequestsShiftTradeBulletinBoardViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			isolate.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
			isolate.UseTestDouble(new FakeCommonAgentNameProvider("{LastName} {FirstName}")).For<ICommonAgentNameProvider>();
		}
	}

	[TestFixture, MyTimeWebRequestsShiftTradeBulletinBoardViewModelFactoryTest]
	public class RequestsShiftTradebulletinViewModelFactoryTest
	{
		public FakePeopleForShiftTradeFinder PeopleForShiftTradeFinder;
		public FakePersonRepository PersonRepository;
		public ICurrentScenario CurrentScenario;
		public ITeamRepository TeamRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeLoggedOnUser LoggedOnUser;
		public IRequestsShiftTradeBulletinViewModelFactory Target;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public ISettingsPersisterAndProvider<NameFormatSettings> Settings;

		private IScenario scenario;
		private ITeam team;
		private IPersonPeriod personPeriod;
		private IPerson me;

		private DateTime baseDateLocal;
		private DateTime baseDateUtc;
		private DateOnly baseDateOnly;

		private void setUpMe()
		{
			baseDateLocal = new DateTime(2016, 01, 13);
			baseDateUtc = DateTime.SpecifyKind(baseDateLocal, DateTimeKind.Utc);
			baseDateOnly = new DateOnly(baseDateLocal);

			scenario = CurrentScenario.Current();
			team = TeamFactory.CreateSimpleTeam("team");
			SiteFactory.CreateSimpleSite("site").AddTeam(team);
			TeamRepository.Add(team);
			personPeriod = PersonPeriodFactory.CreatePersonPeriod(baseDateOnly, team);
			me = PersonFactory.CreatePerson("me");
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);
			
		}

		private void setUpMySchedule()
		{
			var period = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(10));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(me,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);
		}

		private string setUpOffer(IPerson person,
			ShiftExchangeLookingForDay exchangeType = ShiftExchangeLookingForDay.WorkingShift)
		{
			var periodStart = baseDateUtc;
			var periodEnd = baseDateUtc.AddHours(23);
			var period = exchangeType != ShiftExchangeLookingForDay.EmptyDay
				? new DateTimePeriod(periodStart, periodEnd)
				: (DateTimePeriod?) null;

			var criteria = new ShiftExchangeCriteria(DateOnly.Today, new ScheduleDayFilterCriteria(exchangeType, period));
			var offer = new ShiftExchangeOffer(ScheduleDayFactory.Create(baseDateOnly, person, scenario), criteria,
				ShiftExchangeOfferStatus.Pending)
			{
				ShiftExchangeOfferId = Guid.NewGuid().ToString()
			};
			var personRequest = new PersonRequest(person) {Request = offer};
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			PersonRepository.Has(person); 
			PeopleForShiftTradeFinder.Has(new PersonAuthorization{PersonId = person.Id.GetValueOrDefault()});
			return offer.ShiftExchangeOfferId;
		}

		[Test]
		public void ShouldRetrieveMySchedule()
		{
			setUpMe();
			setUpMySchedule();

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Length.Should().Be(1);
			result.MySchedule.Name.Should().Be.EqualTo("me me");
			result.MySchedule.ScheduleLayers[0].Start.Should().Be.EqualTo(baseDateLocal.AddHours(8));
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(baseDateLocal.AddHours(8));
		}

		[Test]
		public void ShouldIndicateOvertimeInMySchedule()
		{
			setUpMe();

			var period1 = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(10));
			var myAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(me,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			var period2 = new DateTimePeriod(baseDateUtc.AddHours(6), baseDateUtc.AddHours(8));
			myAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period2, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleStorage.Add(myAss);

		

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Length.Should().Be(2);
			result.MySchedule.Name.Should().Be.EqualTo("me me");
			result.MySchedule.ScheduleLayers[0].IsOvertime.Should().Be.EqualTo(true);
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(baseDateLocal.AddHours(6));
		}

		[Test]
		public void ShouldRetrieveShiftTradeExchangedSchedules()
		{
			setUpMe();
			setUpMySchedule();
			var someone = PersonFactory.CreatePersonWithGuid("someone", "someone");
			someone.AddPersonPeriod(personPeriod);
			PersonRepository.Add(someone);

			var period = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(10));

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(someone,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleStorage.Add(personAss);

			var offerId = setUpOffer(someone);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be(1);
			result.PossibleTradeSchedules.First().ShiftExchangeOfferId.ToString().Should().Be.EqualTo(offerId);
		}

		[Test]
		public void ShouldNotSeeOtherExchangeOfferSchedulesWhenMyScheduleIsEmpty()
		{
			setUpMe();
			var someone = PersonFactory.CreatePersonWithGuid("someone", "someone");
			someone.AddPersonPeriod(personPeriod);
			PersonRepository.Add(someone);

			var period = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(10));

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(someone,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleStorage.Add(personAss);
			setUpOffer(someone);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(baseDateUtc),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be(0);
		}

		[Test]
		public void ShouldFilterOutDayOffWithFullDayAbsence()
		{
			setUpMe();
			setUpMySchedule();
			var personWithAbsence = PersonFactory.CreatePersonWithGuid("p2", "p2");
			var personWithoutAbsence = PersonFactory.CreatePersonWithGuid("p3", "p3");
			personWithAbsence.AddPersonPeriod(personPeriod);
			personWithoutAbsence.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personWithAbsence);
			PersonRepository.Add(personWithoutAbsence);

			var period = new DateTimePeriod(baseDateUtc, baseDateUtc.AddHours(23));

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithAbsence,
				scenario, baseDateOnly, new DayOffTemplate());
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario, period, abs);
			ScheduleStorage.Add(personAssWithDayOff);
			ScheduleStorage.Add(personAbsence);

			var p3AssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithoutAbsence,
				scenario, baseDateOnly, new DayOffTemplate());
			ScheduleStorage.Add(p3AssWithDayOff);

			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFilterOutFullDayAbsence()
		{
			setUpMe();
			setUpMySchedule();
			var personWithAbsence = PersonFactory.CreatePersonWithGuid("p2", "p2");
			var personWithoutAbsence = PersonFactory.CreatePersonWithGuid("p3", "p3");
			personWithAbsence.AddPersonPeriod(personPeriod);
			personWithoutAbsence.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personWithAbsence);
			PersonRepository.Add(personWithoutAbsence);

			var period = new DateTimePeriod(baseDateUtc, baseDateUtc.AddHours(23));

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario, period, abs);
			ScheduleStorage.Add(personAbsence);

			var personWithoutAbsenceAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithoutAbsence,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleStorage.Add(personWithoutAbsenceAss);

			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFilterOutFullDayAbsenceOnContractDayOff()
		{
			setUpMe();
			setUpMySchedule();
			var personWithAbsence = PersonFactory.CreatePersonWithGuid("p2", "p2");
			var personWithoutAbsence = PersonFactory.CreatePersonWithGuid("p3", "p3");
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			personWithAbsence.AddPersonPeriod(personPeriod);
			personWithoutAbsence.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personWithAbsence);
			PersonRepository.Add(personWithoutAbsence);

			var period = new DateTimePeriod(baseDateUtc, baseDateUtc.AddHours(23));

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario,
				period, abs);
			ScheduleStorage.Add(personAbsence);

			var personWithoutAbsenceAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithoutAbsence,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleStorage.Add(personWithoutAbsenceAss);

			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);
			

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFilterOutOvertimeOnDayOff()
		{
			setUpMe();
			setUpMySchedule();
			var personWithMainShift = PersonFactory.CreatePersonWithGuid("person", "mainShift");
			var personWithOvertimeOnDayOff = PersonFactory.CreatePersonWithGuid("person", "overtime");
			personWithMainShift.AddPersonPeriod(personPeriod);
			personWithOvertimeOnDayOff.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personWithMainShift);
			PersonRepository.Add(personWithOvertimeOnDayOff);

			var period = new DateTimePeriod(baseDateUtc.AddHours(13), baseDateUtc.AddHours(23));

			var personAssWithMainShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			var personAssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithOvertimeOnDayOff,
				scenario, baseDateOnly, new DayOffTemplate());

			personAssWithDayOff.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleStorage.Add(personAssWithMainShift);
			ScheduleStorage.Add(personAssWithDayOff);

			setUpOffer(personWithMainShift);
			setUpOffer(personWithOvertimeOnDayOff);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetriveEmptyShift()
		{
			setUpMe();

			var personWithMainShift = PersonFactory.CreatePersonWithGuid("person", "mainShift");
			var personWithoutSchedule = PersonFactory.CreatePersonWithGuid("person", "empty");
			personWithMainShift.AddPersonPeriod(personPeriod);
			personWithoutSchedule.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personWithMainShift);
			PersonRepository.Add(personWithoutSchedule);

			var period = new DateTimePeriod(baseDateUtc, baseDateUtc.AddHours(23));
			var personAssWithMainShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift, scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			ScheduleStorage.Add(personAssWithMainShift);

			setUpOffer(personWithMainShift);
			setUpOffer(personWithoutSchedule, ShiftExchangeLookingForDay.EmptyDay);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });


			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
			result.PossibleTradeSchedules.First().Name.Should().Be.EqualTo("empty person");
		}

		[Test]
		public void ShouldReturnOrderedSchedules()
		{
			setUpMe();
			setUpMySchedule();

			var person1 = PersonFactory.CreatePersonWithGuid("person1", "mainshift");
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "mainshift");
			var person3 = PersonFactory.CreatePersonWithGuid("person3", "mainshift");
			person1.AddPersonPeriod(personPeriod);
			person2.AddPersonPeriod(personPeriod);
			person3.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var period1 = new DateTimePeriod(baseDateUtc.AddHours(12), baseDateUtc.AddHours(17));
			var person1Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period2 = new DateTimePeriod(baseDateUtc.AddHours(9), baseDateUtc.AddHours(17));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period3 = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(17));
			var person3Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
				scenario, period3, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			ScheduleStorage.Add(person1Ass);
			ScheduleStorage.Add(person2Ass);
			ScheduleStorage.Add(person3Ass);

			setUpOffer(person1);
			setUpOffer(person2);
			setUpOffer(person3);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });



			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(3);
			result.PossibleTradeSchedules.First().Name.Should().Be.EqualTo("mainshift person3");
			result.PossibleTradeSchedules.Second().Name.Should().Be.EqualTo("mainshift person2");
			result.PossibleTradeSchedules.Last().Name.Should().Be.EqualTo("mainshift person1");
		}

		[Test]
		public void ShouldReturnOneSchedulesForDuplicatedShiftTradeOffer()
		{
			setUpMe();
			setUpMySchedule();

			var person1 = PersonFactory.CreatePersonWithGuid("person1", "mainshift");
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "mainshift");
			var person3 = PersonFactory.CreatePersonWithGuid("person3", "mainshift");
			person1.AddPersonPeriod(personPeriod);
			person2.AddPersonPeriod(personPeriod);
			person3.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var period1 = new DateTimePeriod(baseDateUtc.AddHours(12), baseDateUtc.AddHours(17));
			var person1Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period2 = new DateTimePeriod(baseDateUtc.AddHours(9), baseDateUtc.AddHours(17));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period3 = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(17));
			var person3Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
				scenario, period3, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			ScheduleStorage.Add(person1Ass);
			ScheduleStorage.Add(person2Ass);
			ScheduleStorage.Add(person3Ass);

			// Create duplicate shift exchange offers
			setUpOffer(person1);
			setUpOffer(person1);
			setUpOffer(person1);

			setUpOffer(person2);
			setUpOffer(person2);

			setUpOffer(person3);
			setUpOffer(person3);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(3);
			result.PossibleTradeSchedules.First().Name.Should().Be.EqualTo("mainshift person3");
			result.PossibleTradeSchedules.Second().Name.Should().Be.EqualTo("mainshift person2");
			result.PossibleTradeSchedules.Last().Name.Should().Be.EqualTo("mainshift person1");
		}

		[Test]
		public void ShouldReturnOrderedSchedulesForSpecificPage()
		{
			setUpMe();
			setUpMySchedule();

			var person1 = PersonFactory.CreatePersonWithGuid("person1", "mainshift");
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "mainshift");
			var person3 = PersonFactory.CreatePersonWithGuid("person3", "mainshift");
			person1.AddPersonPeriod(personPeriod);
			person2.AddPersonPeriod(personPeriod);
			person3.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var period1 = new DateTimePeriod(baseDateUtc.AddHours(12), baseDateUtc.AddHours(17));
			var person1Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period2 = new DateTimePeriod(baseDateUtc.AddHours(9), baseDateUtc.AddHours(17));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period3 = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(17));
			var person3Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
				scenario, period3, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			ScheduleStorage.Add(person1Ass);
			ScheduleStorage.Add(person2Ass);
			ScheduleStorage.Add(person3Ass);

			setUpOffer(person1);
			setUpOffer(person2);
			setUpOffer(person3);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 1, Take = 1},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
			result.PossibleTradeSchedules.First().Name.Should().Be.EqualTo("mainshift person2");
			result.PageCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnCorrectSchedulesForSpecificPageWithDuplicatedExchangeOffer()
		{
			setUpMe();
			setUpMySchedule();

			var person1 = PersonFactory.CreatePersonWithGuid("person1", "mainshift");
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "mainshift");
			var person3 = PersonFactory.CreatePersonWithGuid("person3", "mainshift");
			person1.AddPersonPeriod(personPeriod);
			person2.AddPersonPeriod(personPeriod);
			person3.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var period1 = new DateTimePeriod(baseDateUtc.AddHours(12), baseDateUtc.AddHours(17));
			var person1Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period2 = new DateTimePeriod(baseDateUtc.AddHours(9), baseDateUtc.AddHours(17));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			var period3 = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(17));
			var person3Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
				scenario, period3, ShiftCategoryFactory.CreateShiftCategory("mainshift"));

			ScheduleStorage.Add(person1Ass);
			ScheduleStorage.Add(person2Ass);
			ScheduleStorage.Add(person3Ass);

			// Create duplicate shift exchange offers
			setUpOffer(person1);
			setUpOffer(person1);
			setUpOffer(person1);

			setUpOffer(person2);
			setUpOffer(person2);

			setUpOffer(person3);
			setUpOffer(person3);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 2, Take = 1},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
			result.PossibleTradeSchedules.First().Name.Should().Be.EqualTo("mainshift person1");
			result.PageCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldMapTimeLine()
		{
			setUpMe();
			setUpMySchedule();
			var someone = PersonFactory.CreatePersonWithGuid("someone", "someone");
			someone.AddPersonPeriod(personPeriod);
			PersonRepository.Add(someone);

			var period = new DateTimePeriod(baseDateUtc.AddHours(8), baseDateUtc.AddHours(10));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(someone,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleStorage.Add(personAss);

			setUpOffer(someone);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = baseDateOnly,
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.TimeLineHours.Count().Should().Be.EqualTo(4);
		}
	}
}