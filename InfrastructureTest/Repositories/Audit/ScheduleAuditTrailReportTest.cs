using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class ScheduleAuditTrailReportTest : AuditTest
	{
		private IScheduleAuditTrailReport target;
		private IUserTimeZone timeZone;
		private IUnsafePersonProvider personProvider;

		protected override void AuditSetup()
		{
			timeZone = new FakeUserTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			target = new ScheduleAuditTrailReport(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), timeZone);
			personProvider = new UnsafePersonProvider();
		}

		[Test]
		public void ShouldFindRevisionPeople()
		{
			IEnumerable<IPerson> revPeople;
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				revPeople = target.RevisionPeople();
			}
			revPeople.Should().Contain(PersonAssignment.UpdatedBy);
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
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100);
				res.Any(pa => consideredEqual(pa, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldFindModifiedAssignmentWithGivenNumberOfResults()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(2)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 100);

				res.Count.Should().Be.GreaterThan(1);

				res = target.Report(personProvider.CurrentUser(),
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(2)),
					PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()), 1);

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
				ModifiedBy = PersonAbsence.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAbsence.Person.Name.ToString(NameOrderOption.FirstNameLastName),
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
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((timeZone.TimeZone())), 100);
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
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName)
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(uow);
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((timeZone.TimeZone())),100);
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
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName)
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
				var rep = new PersonAssignmentRepository(uow);
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
									new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date),100);
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
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((timeZone.TimeZone())),100);
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
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
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
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date),100);
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldShowTimeInUsersTimeZone()
		{
			//changing timezone to -4 GMT
			timeZone = new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
			target = new ScheduleAuditTrailReport(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), timeZone);

			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportInsert,
				ShiftType = Resources.AuditingReportShift,
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, timeZone.TimeZone()),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, timeZone.TimeZone()),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, timeZone.TimeZone())
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today)),
								  PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
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
								  new DateOnlyPeriod(assignmentStart.AddDays(2), assignmentStart.AddDays(10)),100);
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
								  new DateOnlyPeriod(assignmentStart.AddDays(-100), assignmentStart.AddDays(-1)),100);
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
								  PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
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
				var res = target.Report(null, new DateOnlyPeriod(new DateOnly(Today).AddDays(-100), new DateOnly(Today).AddDays(-2)),
								  PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldHaveDetailShiftCategoryWhenHasShiftCategory()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), 
					new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
				res.Select(x => x.Detail).Contains(PersonAssignment.ShiftCategory.Description.Name).Should().Be.EqualTo(true);

			}
		}

		[Test]
		public void ShouldHaveDetailShiftCategoryWhenHasShiftCategoryAndOtherLayers()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
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
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
				res.Select(x => x.Detail).Contains(DayOffTemplate.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOffAndPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);

				res.Select(x => x.Detail).Contains(DayOffTemplate.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOffAndOvertimeShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
				res.Select(x => x.Detail).Contains(DayOffTemplate.Description.Name).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailPersonalShiftWhenOnlyPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);

				res.Select(x => x.Detail).Contains(Resources.PersonalShift).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailOvertimeShiftWhenOnlyOvertimeShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
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
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);

				res.Select(x => x.Detail).Contains(string.Empty).Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldHaveDetailEmptyWhenOverTimeAndPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(), new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(timeZone.TimeZone()),100);
				
				res.Select(x => x.Detail).Contains(string.Empty).Should().Be.EqualTo(true);
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
			if (!semiEqual)
				return false;
			return new TimeSpan(Math.Abs(first.ModifiedAt.Ticks - second.ModifiedAt.Ticks)) < TimeSpan.FromMinutes(1);
		}

	}
}