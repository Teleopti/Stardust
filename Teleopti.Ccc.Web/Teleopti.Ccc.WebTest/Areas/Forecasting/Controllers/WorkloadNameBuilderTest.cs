using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class WorkloadNameBuilderTest
	{
		[TestCase("skill1", "workload1", "skill1 - workload1")]
		[TestCase("skill1", "", "skill1")]
		[TestCase("skill1", null, "skill1")]
		[TestCase("test1", "test1", "test1")]
		public void ShouldCombineSkillNameAndWorkloadName(string skillName, string workloadName, string expectedWorkloadName)
		{
			var result = WorkloadNameBuilder.GetWorkloadName(skillName, workloadName);
			result.Should().Be.EqualTo(expectedWorkloadName);
		}
	}
}