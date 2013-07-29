using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class OvertimeShiftLayerTest
	{
		[Test]
		public void ShouldKnowItsIndex()
		{
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			ass.OvertimeLayers().Last().OrderIndex.Should().Be.EqualTo(2);
		}
	}
}