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
			Assert.IsFalse(_target.IsSatisfiedByWithEqualCondition( 14, 25));
		}

		[Test]
		public void ReturnTrueIfCalculatedSeatsMoreThanMaxSeats()
		{
			Assert.IsTrue(_target.IsSatisfiedByWithEqualCondition(27, 25));
		}

		[Test]
		public void ReturnTrueIfCalculatedSeatsEqualMaxSeats()
		{
			Assert.IsTrue(_target.IsSatisfiedByWithEqualCondition(25, 25));
		}

		[Test]
		public void ReturnFalseIfMaxAndCalAreEqual()
		{
			Assert.IsFalse(_target.IsSatisfiedByWithoutEqualCondition( 14, 14));
		}

		[Test]
		public void ReturnFalseIfMaxSeatNotUsedWithoutEqualCondition()
		{
			Assert.IsFalse( _target.IsSatisfiedByWithEqualCondition(13, 20));
		}

		[Test]
		public void ReturnTrueIfCalSeatMoreThenMaxWithoutEqualCondition()
		{
			Assert.IsTrue(_target.IsSatisfiedByWithEqualCondition(28, 20));
		}
	}
}
