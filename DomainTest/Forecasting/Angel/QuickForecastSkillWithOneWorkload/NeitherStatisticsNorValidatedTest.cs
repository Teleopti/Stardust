using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	[Ignore(@"
Now the skilldays are read before statistic is read and these are the one asserting. 
Should assert change or should real code change? 
Either override currentskilldays and assert nothing has changed or 
in some way jump out earlier in real code?")]
	public class NeitherStatisticsNorValidatedTest : QuickForecastTest
	{
		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			modifiedSkillDays.Should().Be.Empty();
		}
	}
}