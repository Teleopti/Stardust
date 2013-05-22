using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class ScheduleHistoryReportTest : AuditTest
	{
		private IScheduleHistoryReport target;
		private IUnsafePersonProvider personProvider;
		private Regional regional;

		protected override void AuditSetup()
		{
			var culture = CultureInfo.GetCultureInfo("sv-SE");
			regional = new Regional(TimeZoneInfo.Local, culture, culture);
			target = new ScheduleHistoryReport(UnitOfWorkFactory.Current, regional);
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
										ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
										ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
										ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone)
									};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//fix later when mainshift is removed -> add mainshiftlayer directly
				uow.Reassociate(PersonAssignment);
				var ms = PersonAssignment.ToMainShift();
				var sameAct = ms.LayerCollection[0].Payload;
				ms.LayerCollection.Add(new MainShiftActivityLayer(sameAct, new DateTimePeriod(Today, Today.AddDays(1))));
				PersonAssignment.SetMainShift(ms);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person });
				res.Any(pa => consideredEqual(pa, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldFindInsertedPersonDayOffWithCorrectParameters()
		{
			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportInsert,
				ShiftType = Resources.AuditingReportDayOff,
				Detail = PersonDayOff.DayOff.Description.Name,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonDayOff.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonDayOff.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonDayOff.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonDayOff.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonDayOff.Period.StartDateTime, regional.TimeZone)
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Local),
								  new List<IPerson> { PersonAssignment.Person });
				res.Any(dayOff => consideredEqual(dayOff, expected)).Should().Be.True();
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
				var rep = new PersonAbsenceRepository(UnitOfWorkFactory.Current);
				rep.Remove(PersonAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
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
				var rep = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
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
				ScheduleEnd = DateTime.MinValue,
				ScheduleStart = DateTime.MinValue,
				Detail = string.Empty,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName)
			};

			//remove mainshiftlayers
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.ClearMainShiftLayers();
				uow.PersistAll();
			}
			//remove assignment
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
				rep.Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
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
				Detail = string.Empty,
				ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value, regional.TimeZone),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime, regional.TimeZone),
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime, regional.TimeZone)
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				var pShift = new PersonalShift();
				pShift.LayerCollection.Add(new PersonalShiftActivityLayer(PersonAssignment.ToMainShift().LayerCollection[0].Payload,
																							 PersonAssignment.ToMainShift().LayerCollection[0].Period));
				PersonAssignment.ClearMainShiftLayers();
				PersonAssignment.AddPersonalShift(pShift);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
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
				ScheduleEnd = DateTime.MinValue,
				ScheduleStart = DateTime.MinValue,
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.ClearMainShiftLayers();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod((TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldShowTimeInUsersTimeZone()
		{
			//changing timezone to -4 GMT
			regional.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));

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
								  new List<IPerson> { PersonAssignment.Person });
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
								  new List<IPerson> { PersonAssignment.Person });
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
								  new List<IPerson> { PersonAssignment.Person });
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
								  new List<IPerson> { PersonAssignment.Person });
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
								  new List<IPerson> { PersonAssignment.Person });
				res.Should().Be.Empty();
			}
		}
	}
}