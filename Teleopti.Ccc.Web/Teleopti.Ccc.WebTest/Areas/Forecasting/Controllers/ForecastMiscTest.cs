using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastMiscTest
	{
		[Test]
		public void ShouldCombineSkillNameAndWorkloadName()
		{
			var target = new ForecastMisc();
			var result = target.WorkloadName("skill1", "workload1");
			result.Should().Be.EqualTo("skill1 - workload1");
		}

		[Test]
		public void ShouldNotDuplicateIfSkillNameAndWorkloadNameAreSame()
		{
			var target = new ForecastMisc();
			var result = target.WorkloadName("test1", "test1");
			result.Should().Be.EqualTo("test1");
		}

		[Test]
		public void ShouldUseSkillNameIfWorkloadNameIsNullOrEmpty()
		{
			var target = new ForecastMisc();
			var result = target.WorkloadName("test1", "test1");
			result.Should().Be.EqualTo("test1");
		}
	}
}