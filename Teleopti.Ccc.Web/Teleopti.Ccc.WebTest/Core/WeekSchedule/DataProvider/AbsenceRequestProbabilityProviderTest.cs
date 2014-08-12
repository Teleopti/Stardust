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
		
		[Test]
		public void ShouldCalculateYellowIfOneLeft()
		{
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(0), TimeSpan.FromHours(0), 3, true, true);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(0), TimeSpan.FromHours(0), 20, true, true);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(0), TimeSpan.FromHours(0), 2, true, true);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(0), TimeSpan.FromHours(0), 0, true, true);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(24), TimeSpan.FromHours(8), 0, true, false);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(24), TimeSpan.FromHours(8), 0, true, false);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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
			var allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			var absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();
			var target = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, new Now());
			var date = DateOnly.Today;

			var weekButNoWeek = new DateOnlyPeriod(date, date);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>(weekButNoWeek.StartDate, TimeSpan.FromHours(24), TimeSpan.FromHours(8), 0, true, false);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(weekButNoWeek)).Return(new[] { allowanceDay1 });
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