using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalIssuesTest
	{
		private IntraIntervalIssues _target;
		private IList<ISkillStaffPeriod> _skillStaffPeriods;
		private IList<ISkillStaffPeriod> _skillStaffPeriodsAfter;
		
		
		[SetUp]
		public void SetUp()
		{
			_skillStaffPeriods = new List<ISkillStaffPeriod>();
			_skillStaffPeriodsAfter = new List<ISkillStaffPeriod>();
			_target = new IntraIntervalIssues();
		}

		[Test]
		public void ShouldGetAndSetIssues()
		{

			_target.IssuesOnDay = _skillStaffPeriods;
			_target.IssuesOnDayAfter = _skillStaffPeriodsAfter;

			Assert.AreEqual(_target.IssuesOnDay, _skillStaffPeriods);
			Assert.AreEqual(_target.IssuesOnDayAfter, _skillStaffPeriodsAfter);	
		}
	}
}
