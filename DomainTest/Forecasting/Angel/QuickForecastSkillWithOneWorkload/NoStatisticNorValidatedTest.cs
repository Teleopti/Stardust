using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoStatisticNorValidatedTest : QuickForecastTest
	{
		protected override void Assert(IEnumerable<ForecastResultModel> forecastResult)
		{
			forecastResult.Should().Be.Empty();
		}
	}
}