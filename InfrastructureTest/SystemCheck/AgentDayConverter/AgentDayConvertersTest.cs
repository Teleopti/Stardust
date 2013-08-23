using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class AgentDayConvertersTest
	{
		[Test]
		public void ShouldHaveCorrectConvertersForDbManager()
		{
			AgentDayConverters.ForDbManager().Select(x => x.GetType())
			                  .Should().Have.SameValuesAs(typeof (PersonAssignmentDateSetter), typeof(RemoveEmptyAssignments));
		}

		[Test]
		public void ShouldHaveCorrectConvertersForPeople()
		{
			AgentDayConverters.ForPeople().Select(x => x.GetType())
												.Should().Have.SameValuesAs(typeof(PersonAssignmentDateSetter), typeof(PersonAssignmentAuditDateSetter), typeof(PersonTimeZoneSetter));
		}

		[Test]
		public void ShouldRunPersonTimeZoneSetterFirstForPeopleSoAssignmentsAreResetted()
		{
			AgentDayConverters.ForPeople().First().GetType()
			                  .Should().Be.EqualTo(typeof (PersonTimeZoneSetter));
		}

		[Test]
		public void ShouldRunRemoveEmptyAssignmentsFirstForDbManagerSoEmptyAreRemovedBeforeSetter()
		{
			AgentDayConverters.ForDbManager().First().GetType()
			                  .Should().Be.EqualTo(typeof (RemoveEmptyAssignments));
		}
	}
}