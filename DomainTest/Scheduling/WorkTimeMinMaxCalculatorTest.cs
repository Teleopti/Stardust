using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class WorkTimeMinMaxCalculatorTest
	{
		[Test]
		public void ShouldReturnMinMaxWorkTimeFromRuleSetBag()
		{
			var workTimeMinMax = new WorkTimeMinMax
						{
							StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
							EndTimeLimitation = new EndTimeLimitation(new TimeSpan(16, 0, 0), new TimeSpan(18, 0, 0)),
							WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(10, 0, 0))
						};

			var projectionService = new RuleSetProjectionService(new ShiftCreatorService());
			var target = new WorkTimeMinMaxCalculator(projectionService);
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																  new WorkTimeLimitation(), null, null, null,
																  new List<IActivityRestriction>());

			ruleSetBag.Stub(x => x.MinMaxWorkTime(projectionService, DateOnly.Today, effectiveRestriction)).Return(workTimeMinMax);

			var result = target.WorkTimeMinMax(ruleSetBag, DateOnly.Today);

			result.Should().Be.EqualTo(workTimeMinMax);
		}
	}
}
