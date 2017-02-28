﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoStatisticNorValidatedTest : QuickForecastTest
	{
		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			modifiedSkillDays.Single().Tasks
				.Should().Be.EqualTo(0);
		}
	}
}