using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeDateTimePeriodHolderTest
	{
		private OvertimeDateTimePeriodHolder _target;

		[SetUp]
		public void SetUp()
		{
			_target = new OvertimeDateTimePeriodHolder();	
		}

		[Test]
		public void ShouldAddGetDateTimePeriods()
		{
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 2014, 1, 2);
			_target.Add(dateTimePeriod);
			Assert.AreEqual(dateTimePeriod, _target.DateTimePeriods[0]);
		}
	}	
}
