using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ResourceOptimizerPreferencesTest
	{
        private IOptimizerOriginalPreferences _target;
		private SchedulingOptions _userDefinedSchedulingOptions;

		[SetUp]
		public void Setup()
		{
			_userDefinedSchedulingOptions = new SchedulingOptions();
		

			_target = new OptimizerOriginalPreferences(_userDefinedSchedulingOptions);

		}

		[Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_target);
		}
		
		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(_target.SchedulingOptions, _userDefinedSchedulingOptions);
		}
	}
}
