using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class AbsenceRequestProbabilityProviderTest
	{
		private DateOnly date;
		private IAllowanceProvider allowanceProvider;
		private IAbsenceTimeProvider absenceTimeProvider;
		private AbsenceRequestProbabilityProvider target;

		[SetUp]
		public void Setup()
		{
			date = DateOnly.Today;
			allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
		}

		[Test]
		public void ShouldCalculateYellowIfOneLeft()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 0, 0);
			allowanceDay1.AllowanceHeads = 3;
			allowanceDay1.UseHeadCount = true;

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("yellow"));
		}

		[Test]
		public void ShouldCalculateGreenIfManyLeft()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 0, 0);
			allowanceDay1.AllowanceHeads = 20;
			allowanceDay1.UseHeadCount = true;

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("green"));
		}

		[Test]
		public void ShouldCalculateRedIfNoOneLeft()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 0, 0);
			allowanceDay1.AllowanceHeads = 2;
			allowanceDay1.UseHeadCount = true;

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("red"));
		}

		[Test]
		public void ShouldNotCrashIfNoAllowance()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 0, 0);
			allowanceDay1.UseHeadCount = true;

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("red"));
		}

		[Test]
		public void ShouldCalculateYellowOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 24, 8);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(16).TotalMinutes, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("yellow"));
		}

		[Test]
		public void ShouldCalculateGreenOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 24, 8);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(8).TotalMinutes, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("green"));
		}

		[Test]
		public void ShouldCalculateRedOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = createAllowanceDay(weekButNoWeek.StartDate, 24, 8);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(24).TotalMinutes, Date = weekButNoWeek.StartDate.Date}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Date, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().CssClass, Is.EqualTo("red"));
		}

		private IAllowanceDay createAllowanceDay(DateOnly allowanceDate, int timeHours, int headHours)
		{
			return new AllowanceDay
			{
				Date = allowanceDate,
				Time = TimeSpan.FromHours(timeHours),
				Heads = TimeSpan.FromHours(headHours),
				AllowanceHeads = 0,
				Availability = true,
				UseHeadCount = false,
				ValidateBudgetGroup = true
			};
		}
	}
}