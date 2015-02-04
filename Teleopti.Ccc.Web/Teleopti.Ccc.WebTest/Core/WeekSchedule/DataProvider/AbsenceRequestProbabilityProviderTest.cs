using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class AbsenceRequestProbabilityProviderTest
	{
		DateOnly date;
		IAllowanceProvider allowanceProvider;
		IAbsenceTimeProvider absenceTimeProvider;
		AbsenceRequestProbabilityProvider target;

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
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(0),
				Heads = TimeSpan.FromHours(0),
				AllowanceHeads = 3,
				Availability = true,
				UseHeadCount = true
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("yellow"));
		}

		[Test]
		public void ShouldCalculateGreenIfManyLeft()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(0),
				Heads = TimeSpan.FromHours(0),
				AllowanceHeads = 20,
				Availability = true,
				UseHeadCount = true
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("green"));
		}

		[Test]
		public void ShouldCalculateRedIfNoOneLeft()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(0),
				Heads = TimeSpan.FromHours(0),
				AllowanceHeads = 2,
				Availability = true,
				UseHeadCount = true
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("red"));
		}

		[Test]
		public void ShouldNotCrashIfNoAllowance()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(0),
				Heads = TimeSpan.FromHours(0),
				AllowanceHeads = 0,
				Availability = true,
				UseHeadCount = true
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {HeadCounts = 2, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("red"));
		}

		[Test]
		public void ShouldCalculateYellowOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(24),
				Heads = TimeSpan.FromHours(8),
				AllowanceHeads = 0,
				Availability = true,
				UseHeadCount = false
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(16).TotalMinutes, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("yellow"));
		}

		[Test]
		public void ShouldCalculateGreenOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(24),
				Heads = TimeSpan.FromHours(8),
				AllowanceHeads = 0,
				Availability = true,
				UseHeadCount = false
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(8).TotalMinutes, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("green"));
		}

		[Test]
		public void ShouldCalculateRedOnBudgetGroup()
		{
			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new AllowanceDay
			{
				Date = weekButNoWeek.StartDate,
				Time = TimeSpan.FromHours(24),
				Heads = TimeSpan.FromHours(8),
				AllowanceHeads = 0,
				Availability = true,
				UseHeadCount = false
			};

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] {allowanceDay1});
			absenceTimeProvider.Stub(x => x.GetAbsenceTimeForPeriod(weekButNoWeek))
				.Return(new List<IAbsenceAgents>
				{
					new AbsenceAgents {AbsenceTime = TimeSpan.FromHours(24).TotalMinutes, Date = weekButNoWeek.StartDate}
				});

			var ret = target.GetAbsenceRequestProbabilityForPeriod(weekButNoWeek);
			Assert.That(ret.First().Item1, Is.EqualTo(weekButNoWeek.StartDate));
			Assert.That(ret.First().Item2, Is.EqualTo("red"));
		}
	}
}