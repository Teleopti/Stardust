using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class ConvertScheduleTest
	{
		[Test]
		public void ShouldCallAllConverters()
		{
			var converter1 = MockRepository.GenerateMock<IPersonAssignmentConverter>();
			var converter2 = MockRepository.GenerateMock<IPersonAssignmentConverter>();
			var target = new ConvertSchedule(new[] {converter1, converter2});

			target.ExecuteAllConverters();

			converter1.AssertWasCalled(x => x.Execute());
			converter2.AssertWasCalled(x => x.Execute());
		}
	}
}