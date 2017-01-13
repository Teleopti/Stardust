using Teleopti.Ccc.Domain.Common.TimeLogger;

namespace Teleopti.Ccc.DomainTest.Common.TimeLogger
{
	public class LogTimeTester
	{
		[TestLog]
		public virtual void TestMethod()
		{
		}

		[TestLog]
		public virtual void TestMethodThatThrows()
		{
			throw new System.NotImplementedException();
		}
	}
}