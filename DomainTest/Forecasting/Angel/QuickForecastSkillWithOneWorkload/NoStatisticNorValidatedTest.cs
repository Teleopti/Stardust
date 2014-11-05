using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	[Ignore("Currently throws. Maybe it should but then it should be a nicer exception...")]
	public class NoStatisticNorValidatedTest : QuickForecastTest
	{
		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			//robin - what should happen?
		}
	}
}