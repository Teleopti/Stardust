using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Aop
{
	public class EventHandlersNeedsToBeVirtualTest
	{
		[Test]
		public void MethodMustBeVirtualOnStardustHandleMethod()
		{
			foreach (var type in typeof(Person).Assembly.GetTypes()
				.Where(x => x.IsClass && typeof(IRunOnStardust).IsAssignableFrom(x)))
			{
				foreach (var method in type.GetMethods().Where(x => x.Name.Equals("Handle")))
				{
					if (!method.IsVirtual || method.IsFinal)
					{
						Assert.Fail("Handle method on {0} needs to be virtual. If not, toggle refresh wont work!", type.Name);
					}
				}	
			}
		}
	}
}