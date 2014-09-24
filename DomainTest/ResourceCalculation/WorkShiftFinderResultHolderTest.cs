using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class WorkShiftFinderResultHolderTest
	{
		private IWorkShiftFinderResultHolder _target;

		[SetUp]
		public void Setup()
		{
			_target = new WorkShiftFinderResultHolder();
		}

		[Test]
		public void ShouldResetAllwaysShowTroubleShootOnClear()
		{
			_target.AlwaysShowTroubleshoot = true;
			_target.Clear();
			Assert.IsFalse(_target.AlwaysShowTroubleshoot);
		}

	}
}