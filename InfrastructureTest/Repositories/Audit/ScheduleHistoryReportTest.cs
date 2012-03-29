using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
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
			regional = new Regional(new CccTimeZoneInfo(TimeZoneInfo.Local), null, null);
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
										Detail = PersonAssignment.MainShift.ShiftCategory.Description.Name,
										ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
										ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
										ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime),
										ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime)
									};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var sameAct = PersonAssignment.MainShift.LayerCollection[0].Payload;
				uow.Reassociate(PersonAssignment);
				PersonAssignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(sameAct, new DateTimePeriod(Today, Today.AddDays(1))));
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonDayOff.UpdatedOn.Value),
				ModifiedBy = PersonDayOff.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonDayOff.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonDayOff.Period.EndDateTime),
				ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonDayOff.Period.StartDateTime)
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAbsence.UpdatedOn.Value),
				ModifiedBy = PersonAbsence.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAbsence.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonAbsence.Period.EndDateTime),
				ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonAbsence.Period.StartDateTime)
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
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime),
				ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime),
				Detail = PersonAssignment.MainShift.ShiftCategory.Description.Name,
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
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
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName)
			};

			//remove mainshift
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
				PersonAssignment.ClearMainShift(rep);
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
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime),
				ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime)
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
				var pShift = new PersonalShift();
				pShift.LayerCollection.Add(new PersonalShiftActivityLayer(PersonAssignment.MainShift.LayerCollection[0].Payload,
				                                                          PersonAssignment.MainShift.LayerCollection[0].Period));
				PersonAssignment.ClearMainShift(rep);
				PersonAssignment.AddPersonalShift(pShift);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = DateTime.MinValue,
				ScheduleStart = DateTime.MinValue,
			};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
				PersonAssignment.ClearMainShift(rep);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(personProvider.CurrentUser(),
								  new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
								  new List<IPerson> { PersonAssignment.Person });
				res.Any(absence => consideredEqual(absence, expected)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldShowTimeInUsersTimeZone()
		{
			//changing timezone to -4 GMT
			regional.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));

			var expected = new ScheduleAuditingReportData
			{
				AuditType = Resources.AuditingReportInsert,
				ShiftType = Resources.AuditingReportShift,
				Detail = PersonAssignment.MainShift.ShiftCategory.Description.Name,
				ModifiedAt = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.UpdatedOn.Value),
				ModifiedBy = PersonAssignment.UpdatedBy.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduledAgent = PersonAssignment.Person.Name.ToString(NameOrderOption.FirstNameLastName),
				ScheduleEnd = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.EndDateTime),
				ScheduleStart = regional.TimeZone.ConvertTimeFromUtc(PersonAssignment.Period.StartDateTime)
			};

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var res = target.Report(new DateOnlyPeriod(new DateOnly(Today).AddDays(-1), new DateOnly(Today).AddDays(1)),
								  PersonAssignment.Period.ToDateOnlyPeriod(new CccTimeZoneInfo(TimeZoneInfo.Local)),
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
	}
}