using Teleopti.Ccc.Domain.Aop;

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