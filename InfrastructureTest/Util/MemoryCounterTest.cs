using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.InfrastructureTest.Util
{
	[TestFixture]
	public class MemoryCounterTest
	{
		private MemoryCounter _target;

		[SetUp]
		public void Setup()
		{
			_target = new MemoryCounter();
		}

		[Test]
		public void VerifyResultIsValidString()
		{
			Assert.IsNotNullOrEmpty(_target.CurrentMemoryConsumption());
		}
	}
}
