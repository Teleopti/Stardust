using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoDataAvailableForMeasurementTest : QuickForecastTest
	{
		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			MeasurementResult.Should().Be.EqualTo(Double.NaN);
		}
	}
}