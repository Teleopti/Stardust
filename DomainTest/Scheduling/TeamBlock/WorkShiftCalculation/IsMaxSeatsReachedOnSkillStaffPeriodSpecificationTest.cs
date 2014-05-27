using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class IsMaxSeatsReachedOnSkillStaffPeriodSpecificationTest
	{
		private IsMaxSeatsReachedOnSkillStaffPeriodSpecification _target;

		[SetUp]
		public void SetUp()
		{
			_target = new IsMaxSeatsReachedOnSkillStaffPeriodSpecification();
		}

		[Test]
		public void ReturnFalseIfMaxSeatsNotReached()
		{
			Assert.IsFalse(_target.IsSatisfiedBy(14, 25));
		}

		[Test]
		public void ReturnTrueIfCalculatedSeatsMoreThanMaxSeats()
		{
			Assert.IsTrue(_target.IsSatisfiedBy(27, 25));
		}

		[Test]
		public void ReturnTrueIfCalculatedSeatsEqualMaxSeats()
		{
			Assert.IsTrue(_target.IsSatisfiedBy(25, 25));
		}
	}
}
