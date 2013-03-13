using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		[Test]
		public void ShouldExist()
		{
			var target = new AddFullDayAbsenceCommandHandler();
			target.Handle(new AddFullDayAbsenceCommand());
		}
	}
}