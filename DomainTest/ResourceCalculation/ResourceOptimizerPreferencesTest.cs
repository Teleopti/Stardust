using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ResourceOptimizerPreferencesTest
	{
        private IOptimizerOriginalPreferences _target;
		private MockRepository _mocks;
		private ISchedulingOptions _userDefinedSchedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_userDefinedSchedulingOptions = _mocks.PartialMock<SchedulingOptions>();
		

			_target = new OptimizerOriginalPreferences(_userDefinedSchedulingOptions);

		}

		[Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifyConstructorOverload()
		{
			Assert.AreEqual(_target.SchedulingOptions, _userDefinedSchedulingOptions);

		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(_target.SchedulingOptions, _userDefinedSchedulingOptions);
		}
	}
}
