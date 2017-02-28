using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class TrueFalseRandomizerTest
	{
		private ITrueFalseRandomizer _target;

		[SetUp]
		public void Setup()
		{
			_target = new TrueFalseRandomizer();
		}

		[Test]
		public void ShouldRandomize()
		{
			var result = Enumerable.Range(0, 100).Select(i => _target.Randomize()).Where(r => r).ToArray();
			
			Assert.IsTrue(result.Length > 25);
		}
	}
}