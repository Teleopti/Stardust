using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	public class DefaultScenarioForStudentAvailabilityScheduleProviderTest
	{
		private DefaultScenarioForStudentAvailabilityScheduleProvider _target;
		private IScheduleProvider _scheduleProvider;

		[Test]
		public void ShouldGetStudentAvailabilityForDate()
		{
			var scheduleDays = new[]
			{
				MockRepository.GenerateMock<IScheduleDay>(),
				MockRepository.GenerateMock<IScheduleDay>()
			};

			_scheduleProvider = new FakeScheduleProvider(scheduleDays);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[]
			{MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(personRestrictions);
			studentAvailabilityDay.Stub(x => x.RestrictionCollection)
				.Return(
					new ReadOnlyCollection<IStudentAvailabilityRestriction>(
						new List<IStudentAvailabilityRestriction>(new[] {studentAvailabilityRestriction})));

			var result = _target.GetStudentAvailabilityForDate(scheduleDays, date);

			result.Should().Be.SameInstanceAs(studentAvailabilityRestriction);
		}

		[Test]
		public void ShouldThrowExceptionWhenStudentAvailabilityForDateHasSeveralRestrictions()
		{
			var scheduleDays = new[]
			{
				MockRepository.GenerateMock<IScheduleDay>()
			};

			_scheduleProvider = new FakeScheduleProvider(scheduleDays);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[]
			{MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(personRestrictions);
			studentAvailabilityDay.Stub(x => x.RestrictionCollection)
				.Return(
					new ReadOnlyCollection<IStudentAvailabilityRestriction>(
						new List<IStudentAvailabilityRestriction>(new[] {studentAvailabilityRestriction, studentAvailabilityRestriction})));

			Assert.Throws<MoreThanOneStudentAvailabilityFoundException>(
				() => _target.GetStudentAvailabilityForDate(scheduleDays, date));
		}

		[Test]
		public void ShouldReturnNullWhenNoStudentAvailabilityForDate()
		{
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var date = DateOnly.Today;

			_scheduleProvider = new FakeScheduleProvider(scheduleDay);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			scheduleDay.Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDay.Stub(x => x.PersonRestrictionCollection())
				.Return(new IScheduleData[0]);

			var result = _target.GetStudentAvailabilityForDate(new[] {scheduleDay}, date);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenNoStudentAvailabilityRestrictionForDate()
		{
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var date = DateOnly.Today;

			_scheduleProvider = new FakeScheduleProvider(scheduleDay);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			scheduleDay.Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDay.Stub(x => x.PersonRestrictionCollection())
				.Return(new [] { new StudentAvailabilityDay(null, date, new List<IStudentAvailabilityRestriction>()) });

			var result = _target.GetStudentAvailabilityForDate(new[] {scheduleDay}, date);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldNotThrowWhen2ScheduleDaysAreFound()
		{
			var scheduleDays = new[]
			{
				MockRepository.GenerateMock<IScheduleDay>(),
				MockRepository.GenerateMock<IScheduleDay>()
			};

			_scheduleProvider = new FakeScheduleProvider(scheduleDays);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[]
			{MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(personRestrictions);
			scheduleDays[1].Stub(x => x.PersonRestrictionCollection()).Return(new IScheduleData[0]);
			studentAvailabilityDay.Stub(x => x.RestrictionCollection)
				.Return(
					new ReadOnlyCollection<IStudentAvailabilityRestriction>(
						new List<IStudentAvailabilityRestriction>(new[] {studentAvailabilityRestriction})));

			_target.GetStudentAvailabilityForDate(scheduleDays, date);
		}

		[Test]
		public void ShouldThrowWhenTotally2StudentAvailabilityRestrictionsFound()
		{
			var scheduleDays = new[]
			{
				MockRepository.GenerateMock<IScheduleDay>(),
				MockRepository.GenerateMock<IScheduleDay>()
			};

			_scheduleProvider = new FakeScheduleProvider(scheduleDays);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[]
			{MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(personRestrictions);
			scheduleDays[1].Stub(x => x.PersonRestrictionCollection()).Return(personRestrictions);
			studentAvailabilityDay.Stub(x => x.RestrictionCollection)
				.Return(
					new ReadOnlyCollection<IStudentAvailabilityRestriction>(
						new List<IStudentAvailabilityRestriction>(new[] {studentAvailabilityRestriction})));

			Assert.Throws<MoreThanOneStudentAvailabilityFoundException>(
				() => _target.GetStudentAvailabilityForDate(scheduleDays, date));
		}


		[Test]
		public void ShouldGetStudentAvailabilityDayForDate()
		{
			var date = DateOnly.Today;
			var period = new DateOnlyPeriod(date, date);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var person = MockRepository.GenerateMock<IPerson>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			_scheduleProvider = new FakeScheduleProvider(scheduleDay);
			_target = new DefaultScenarioForStudentAvailabilityScheduleProvider(_scheduleProvider);

			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleRange.Stub(x => x.ScheduledDayCollection(period)).Return(new[] {scheduleDay});

			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[]
			{MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};

			scheduleDay.Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDay.Stub(x => x.PersonRestrictionCollection())
				.Return(personRestrictions);

			var result = _target.GetStudentAvailabilityDayForDate(date);

			result.Should().Be.SameInstanceAs(studentAvailabilityDay);
		}
	}
}