using Teleopti.Ccc.Domain.Common.TimeLogger;

namespace Teleopti.Ccc.DomainTest.Common.TimeLogger
{
	public class LogTimeTester
	{
		[LogTime]
		public virtual void TestMethod()
		{
		}

		[LogTime]
		public virtual void TestMethodThatThrows()
		{
			throw new System.NotImplementedException();
		}
	}
}