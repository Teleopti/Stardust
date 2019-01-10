using System;
using System.Collections.Generic;
using System.Globalization;
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
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class ScheduleHistoryReportTest : AuditTest
	{
		private IScheduleHistoryReport target;
		private Regional regional;
		private IPerson personProvider;

		protected override void AuditSetup()
		{
			var culture = CultureInfo.GetCultureInfo("sv-SE");
			regional = new Regional(TimeZoneInfo.Local, culture, culture);
			target = new ScheduleHistoryReport(UnitOfWorkFactory.Current, regional);
			personProvider = SetupFixtureForAssembly.loggedOnPerson;
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
										ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
										ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
										ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone)
									};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, new DateTimePeriod(Today, Today.AddDays(1)));
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person },100);
				res.Any(pa => consideredEqual(pa, expected)).Should().Be.True();
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
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAbsence.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAbsence.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAbsence.Period.StartDateTime, regional.TimeZone)
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
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone),
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
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
				var res = target.Report(personProvider,
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				ScheduleStart = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date, regional.TimeZone),
				ScheduleEnd = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date.AddDays(1), regional.TimeZone),
				Detail = string.Empty,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
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
				var res = target.Report(personProvider,
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
									new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone)
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleStart = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date, regional.TimeZone),
				ScheduleEnd = TimeZoneHelper.ConvertFromUtc(PersonAssignment.Date.Date.AddDays(1), regional.TimeZone)
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
								  new DateOnlyPeriod(PersonAssignment.Date, PersonAssignment.Date),
								  new List<IPerson> { PersonAssignment.Person },100);
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldShowTimeInUsersTimeZone()
		{
			//changing timezone to -4 GMT
			regional = new Regional(TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"), regional.Culture, regional.UICulture);
			target = new ScheduleHistoryReport(UnitOfWorkFactory.Current, regional);

			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportInsert,
				ShiftType = Resources.AuditingReportShift,
				Detail = PersonAssignment.ShiftCategory.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone)
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person },100);
				res.Any(dayOff => consideredEqual(dayOff, expected)).Should().Be.True();
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

		[Test]
		public void ShouldNotFindTooEarlyShift()
		{
			var assignmentStart = new DateOnly(PersonAssignment.Period.StartDateTimeLocal(regional.TimeZone));

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  new DateOnlyPeriod(assignmentStart.AddDays(2), assignmentStart.AddDays(10)),
								  new List<IPerson> { PersonAssignment.Person },100);
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindTooLateShift()
		{
			var assignmentStart = new DateOnly(PersonAssignment.Period.StartDateTimeLocal(regional.TimeZone));

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  new DateOnlyPeriod(assignmentStart.AddDays(-100), assignmentStart.AddDays(-1)),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today).AddDays(2), new DateOnly(Today).AddDays(100)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person },100);
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
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today).AddDays(-100), new DateOnly(Today).AddDays(-2)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person },100);
				res.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldHaveDetailShiftCategoryWhenHasShiftCategory()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider,new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(0).Detail, PersonAssignment.ShiftCategory.Description.Name);
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
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, PersonAssignment.ShiftCategory.Description.Name);
			}
		}

		[Test]
		public void ShouldHaveDetailDayOffWhenDayOff()
		{
			// All MainActivities will be cleared on SetDayOff, then length of PersonAssignment.Period will be 0;
			// so save it before that.
			// Refer to bug #48576
			var localPeriod = PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.SetDayOff(DayOffTemplate);
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var period = new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1));
				var res = target.Report(personProvider, period, localPeriod,
					new List<IPerson> {PersonAssignment.Person}, 100);

				Assert.AreEqual(res.ElementAt(1).Detail, DayOffTemplate.Description.Name);
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
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, DayOffTemplate.Description.Name);
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
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, DayOffTemplate.Description.Name);
			}		
		}

		[Test]
		public void ShouldHaveDetailPersonalShiftWhenOnlyPersonalShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddPersonalActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, Resources.PersonalShift);
			}
		}

		[Test]
		public void ShouldHaveDetailOvertimeShiftWhenOnlyOvertimeShift()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddOvertimeActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period, MultiplicatorDefinitionSet);
				PersonAssignment.ClearMainActivities();
				uow.Merge(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, Resources.Overtime);
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
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, string.Empty);
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
				var res = target.Report(personProvider, new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
							PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local), new List<IPerson> { PersonAssignment.Person },100);

				Assert.AreEqual(res.ElementAt(1).Detail, string.Empty);
			}
		}
	}
}