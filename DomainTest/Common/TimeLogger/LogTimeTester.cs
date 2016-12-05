using Teleopti.Ccc.Domain.Common.TimeLogger;

namespace Teleopti.Ccc.DomainTest.Common.TimeLogger
{
	public class LogTimeTester
	{
		[TestLogTime]
		public virtual void TestMethod()
		{
		}

		[TestLogTime]
		public virtual void TestMethodThatThrows()
		{
			throw new System.NotImplementedException();
		}
	}
}