using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class ScheduleAuditTrailReportTest : AuditTest
	{
		private const string nameAliasFormat = "{LastName}^_^{FirstName}";

		private IScheduleAuditTrailReport target;
		private IUserTimeZone timeZone;
		private IPerson personProvider;
		private FakeGlobalSettingDataRepository settingRepository;

		protected override void AuditSetup()
		{
			timeZone = new FakeUserTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			settingRepository = new FakeGlobalSettingDataRepository();
			settingRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting
				{
					AliasFormat = nameAliasFormat
				});

			target = new ScheduleAuditTrailReport(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), timeZone,
				settingRepository);
			personProvider = SetupFixtureForAssembly.loggedOnPerson;
		}

		[Test]
		public void ShouldFindRevisionPeople()
		{
			IEnumerable<SimplestPersonInfo> revPeople;

			var searchPeriod = new DateOnlyPeriod(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				revPeople = target.GetRevisionPeople(searchPeriod);
			}

			var expectedPerson = PersonAssignment.UpdatedBy;
			var person = revPeople.SingleOrDefault(x => x.Id == expectedPerson.Id.Value);
			person.Name.Should().Be.EqualTo(getExpectedName(expectedPerson));
		}

		[Test]
		public void ShouldNotFindRevisionPeople()
		{
			IEnumerable<SimplestPersonInfo> revPeople;
			var searchPeriod = new DateOnlyPeriod(DateTime.Today.AddDays(-4), DateTime.Today.AddDays(-3));
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				revPeople = target.GetRevisionPeople(searchPeriod);
			}

			revPeople.Count().Should().Be(0);
		}

		[Test]
		public void ShouldFindModifiedAssignmentWithCorrectParameters()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportModified,
				ShiftType = Resources.AuditingReportShift,
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload,
					new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Any(pa => consideredEqual(pa, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldNotFindAnythingWhenNoMatchingPerson()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload,
					new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var notMatchingPerson = PersonFactory.CreatePersonWithGuid("John", "Smith");
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {notMatchingPerson});
				res.Count.Should().Be.EqualTo(0);
			}
		}

		[Test]
		public void ShouldFindModifiedAssignmentWithGivenNumberOfResults()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload,
					new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(2)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Count.Should().Be.GreaterThan(1);

				res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(2)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 1, new List<IPerson> {PersonAssignment.Person});

				res.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldFindDeletedPersonAbsenceWithCorrectParameters()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportDeleted,
				ShiftType = Resources.AuditingReportAbsence,
				Detail = PersonAbsence.Layer.Payload.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAbsence.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAbsence.Person),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.Period.StartDateTime, timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAbsenceRepository(new ThisUnitOfWork(uow));
				rep.Remove(PersonAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldFindDeletedAssignment()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportDeleted,
				ShiftType = Resources.AuditingReportShift,
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone()),
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person)
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(new ThisUnitOfWork(uow));
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod((timeZone.TimeZone())), 100, new List<IPerson> {PersonAssignment.Person});
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldHaveBlankShiftCategoryWhenFirstDeleteMainShiftAndThenAssignment()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportDeleted,
				ShiftType = Resources.AuditingReportShift,
				ScheduleStart = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date, timeZone.TimeZone()),
				ScheduleEnd = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date.AddDays(1), timeZone.TimeZone()),
				Detail = string.Empty,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person)
			};

			//remove mainshiftlayers
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			//remove assignment
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(new ThisUnitOfWork(uow));
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date), 100,
					new List<IPerson> {PersonAssignment.Person});
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldFindAssignmentWithoutMainShiftButWithOtherShift()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportModified,
				ShiftType = Resources.AuditingReportShift,
				Detail = Resources.PersonalShift,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Any(schedule => consideredEqual(schedule, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldFindAssignmentWithoutNoShift()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportModified,
				ShiftType = Resources.AuditingReportShift,
				Detail = string.Empty,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person),
				ScheduleStart = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date, timeZone.TimeZone()),
				ScheduleEnd = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date.AddDays(1), timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date), 100,
					new List<IPerson> {PersonAssignment.Person});
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldShowTimeInUsersTimeZone()
		{
			//changing timezone to -4 GMT
			timeZone = new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
			target = new ScheduleAuditTrailReport(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), timeZone,
				settingRepository);

			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportInsert,
				ShiftType = Resources.AuditingReportShift,
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = getExpectedName(PersonAssignment.UpdatedBy),
				ScheduledAgent = getExpectedName(PersonAssignment.Person),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Any(dayOff => consideredEqual(dayOff, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldNotFindTooEarlyShift()
		{
			var assignmentStart = new DateOnly(PersonAssignment.Period.StartDateTimeLocal(timeZone.TimeZone()));

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					new DateOnlyPeriod(assignmentStart.AddDays(2), assignmentStart.AddDays(10)), 100,
					new List<IPerson> {PersonAssignment.Person});
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindTooLateShift()
		{
			var assignmentStart = new DateOnly(PersonAssignment.Period.StartDateTimeLocal(timeZone.TimeZone()));

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					new DateOnlyPeriod(assignmentStart.AddDays(-100), assignmentStart.AddDays(-1)), 100,
					new List<IPerson> {PersonAssignment.Person});
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindTooEarlyModification()
		{
			//"Should" be 1 day to be correct but...
			//need to have 2 days forward here because of timezone issues around midnight.
			//add another test for that soon
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today).AddDays(2), new DateOnly(Today).AddDays(100)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindTooLateModification()
		{
			//"Should" be 1 day to be correct but...
			//need to have 2 days forward here because of timezone issues around midnight.
			//add another test for that soon
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null,
					new DateOnlyPeriod(new DateOnly(Today).AddDays(-100), new DateOnly(Today).AddDays(-2)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldHaveDetailShiftCategoryWhenHasShiftCategory()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(PersonAssignment.ShiftCategory.Description.Name).Should().Be.EqualTo(true);

			}
		}

		[Test]
		public void ShouldHaveDetailShiftCategoryWhenHasShiftCategoryAndOtherLayers()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(PersonAssignment.ShiftCategory.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOff()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(DayOffTemplate.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOffAndPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(DayOffTemplate.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOffAndOvertimeShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(Resources.Overtime).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailPersonalShiftWhenOnlyPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(Resources.PersonalShift).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailOvertimeShiftWhenOnlyOvertimeShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(Resources.Overtime).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailEmptyWhenEmptyPersonAssignment()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(string.Empty).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailEmptyWhenOverTimeAndPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(string.Empty).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailOvertimeShiftWhenOvertimeAddedOnEmptyPersonAssignment()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload,
					PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100, new List<IPerson> {PersonAssignment.Person});

				res.Select(x => x.Detail).Contains(Resources.Overtime).Should().Be.EqualTo(true);
			}
		}

		private static bool consideredEqual(ScheduleAuditingReportData first, ScheduleAuditingReportData second)
		{
			var semiEqual = first.AuditType == second.AuditType &&
							first.Detail == second.Detail &&
							first.ModifiedBy == second.ModifiedBy &&
							first.ScheduledAgent == second.ScheduledAgent &&
							first.ScheduleStart == second.ScheduleStart &&
							first.ScheduleEnd == second.ScheduleEnd &&
							first.ShiftType == second.ShiftType;
			return semiEqual && new TimeSpan(Math.Abs(first.ModifiedAt.Ticks - second.ModifiedAt.Ticks)) <
				   TimeSpan.FromMinutes(1);
		}

		private static string getExpectedName(IPerson person)
		{
			var nameFormat = nameAliasFormat.Replace("{FirstName}", "{0}").Replace("{LastName}", "{1}");
			return string.Format(nameFormat, person.Name.FirstName, person.Name.LastName);
		}

	}
}