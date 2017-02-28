using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	public class StaffingThresholdWithShrinkageValidatorTest
	{
		private IAbsenceRequestValidator _target;

		[SetUp]
		public void Setup()
		{
			_target = new StaffingThresholdWithShrinkageValidator();
		}
		
		[Test]
		public void VerifyCanCreateNewInstance()
		{
			var newInstance = _target.CreateInstance();
			Assert.AreNotSame(_target, newInstance);
			Assert.IsInstanceOf<StaffingThresholdWithShrinkageValidator>(newInstance);
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(UserTexts.Resources.IntradayWithShrinkage, _target.DisplayText);
		}
		
		[Test]
		public void VerifyEquals()
		{
			var otherValidatorOfSameKind = new StaffingThresholdWithShrinkageValidator();
			var otherValidator = new AbsenceRequestNoneValidator();

			Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
			Assert.IsFalse(_target.Equals(otherValidator));
		}

		[Test]
		public void ShouldGetHashCodeInReturn()
		{
			var result = _target.GetHashCode();
			Assert.IsNotNull(result);
		}
	}
}
