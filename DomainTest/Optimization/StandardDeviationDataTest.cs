using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class StandardDeviationDataTest
	{
        private IStandardDeviationData _target;

		[SetUp]
		public void Setup()
		{
            _target = new StandardDeviationData();
		}
		
		[Test]
		public void ShouldHoldStandardDeviationData()
		{
			var dateOnly = new DateOnly();
			_target.Add(dateOnly, 10);

			Assert.That(_target.Data.Count, Is.EqualTo(1));
			Assert.That(_target.Data[dateOnly].Value, Is.EqualTo(10));
		}
	}
}
