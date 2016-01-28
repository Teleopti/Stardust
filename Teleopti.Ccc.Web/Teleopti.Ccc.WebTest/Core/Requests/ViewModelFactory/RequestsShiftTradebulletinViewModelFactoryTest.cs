using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	public class MyTimeWebRequestsShiftTradeBulletinBoardViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakeScheduleRepository>().For<IScheduleRepository>();
			system.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
		}
	}
	[TestFixture, MyTimeWebRequestsShiftTradeBulletinBoardViewModelFactoryTest]
	public class RequestsShiftTradebulletinViewModelFactoryTest
	{
		public FakePersonRepository PersonRepository;
		public FakeCurrentScenario CurrentScenario;
		public ITeamRepository TeamRepository;
		public IScheduleRepository ScheduleRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public IRequestsShiftTradeBulletinViewModelFactory Target;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;

		private IScenario scenario;
		private ITeam team;
		private IPersonPeriod personPeriod;
		private IPerson me;
		private void setUpMe()
		{
			scenario = CurrentScenario.Current();
			team = TeamFactory.CreateSimpleTeam("team");
			TeamRepository.Add(team);
			personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me = PersonFactory.CreatePerson("me");
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);
			
		}

		private void setUpMySchedule()
		{
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, me,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleRepository.Add(personAss);
		}

		private string setUpOffer(IPerson person)
		{
			var criteria = new ShiftExchangeCriteria(DateOnly.Today,
				new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift,
					new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
						DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc))));
			var offer = new ShiftExchangeOffer(ScheduleDayFactory.Create(new DateOnly(2016, 1, 13), person, scenario), criteria, ShiftExchangeOfferStatus.Pending);
			offer.ShiftExchangeOfferId = Guid.NewGuid().ToString();
			var personRequest = new PersonRequest(person) { Request = offer };
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			return offer.ShiftExchangeOfferId;
		}
		[Test]
		public void ShouldRetrieveMySchedule()
		{
			setUpMe();
			setUpMySchedule();
			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.MySchedule.ScheduleLayers.Count().Should().Be(1);
			result.MySchedule.Name.Should().Be.EqualTo("me@me");
			result.MySchedule.ScheduleLayers[0].Start.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
		}

		[Test]
		public void ShouldRetrieveShiftTradeExchangedSchedules()
		{
			setUpMe();
			setUpMySchedule();
			var someone = PersonFactory.CreatePersonWithGuid("someone","someone");
			someone.AddPersonPeriod(personPeriod);
			PersonRepository.Add(someone);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, someone,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc)),ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleRepository.Add(personAss);
			
			var offerId = setUpOffer(someone);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, someone,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleRepository.Add(personAss);
			setUpOffer(someone);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithAbsence,
				new DateOnly(2016, 1, 13), new DayOffTemplate());
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleRepository.Add(personAssWithDayOff);
			ScheduleRepository.Add(personAbsence);

			var p3AssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithoutAbsence,
				new DateOnly(2016, 1, 13), new DayOffTemplate());
			ScheduleRepository.Add(p3AssWithDayOff);

			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleRepository.Add(personAbsence);

			var personWithoutAbsenceAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,personWithoutAbsence,
							new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleRepository.Add(personWithoutAbsenceAss);
			
			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsence, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleRepository.Add(personAbsence);

			var personWithoutAbsenceAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,personWithoutAbsence,
							new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			ScheduleRepository.Add(personWithoutAbsenceAss);
			
			setUpOffer(personWithAbsence);
			setUpOffer(personWithoutAbsence);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var personAssWithMainShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personWithMainShift,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainshift"));
			var personAssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithOvertimeOnDayOff,
				new DateOnly(2016, 1, 13), new DayOffTemplate());
			personAssWithDayOff.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"), new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), new MultiplicatorDefinitionSet("a",MultiplicatorType.Overtime));
			ScheduleRepository.Add(personAssWithMainShift);
			ScheduleRepository.Add(personAssWithDayOff);

			setUpOffer(personWithMainShift);
			setUpOffer(personWithOvertimeOnDayOff);

			var result = Target.CreateShiftTradeBulletinViewModelFromRawData(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}


	}
}
