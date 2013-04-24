using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class AgentDayDateSetterTest
	{
		private IAgentDayDateSetter target;

		[SetUp]
		public void Setup()
		{
			target = new AgentDayDateSetter(new IPersonAssignmentConverter[] { new PersonAssignmentAuditDateSetter(), new PersonAssignmentDateSetter() });
		}
	}
}