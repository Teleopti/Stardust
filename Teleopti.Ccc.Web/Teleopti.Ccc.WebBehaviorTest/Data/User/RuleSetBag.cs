using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class RuleSetBag : IUserDataSetup
	{
		private readonly int _earliestStart;
		private readonly int _latestStart;
		private readonly int _earliestEnd;
		private readonly int _latestEnd;
		public Domain.Scheduling.ShiftCreator.RuleSetBag TheRuleSetBag;
		
		public RuleSetBag(int earliestStart, int latestStart, int earliestEnd, int latestEnd)
		{
			_earliestStart = earliestStart;
			_latestStart = latestStart;
			_earliestEnd = earliestEnd;
			_latestEnd = latestEnd;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var start = new TimePeriodWithSegment(new TimePeriod(_earliestStart, 0, _latestStart, 0), new TimeSpan(0, 15, 0));
			var end = new TimePeriodWithSegment(new TimePeriod(_earliestEnd, 0, _latestEnd, 0), new TimeSpan(0, 15, 0));
			TheRuleSetBag = new Domain.Scheduling.ShiftCreator.RuleSetBag();
			var generator = new WorkShiftTemplateGenerator(TestData.ActivityPhone, start, end, TestData.ShiftCategory);
			var ruleSet = new WorkShiftRuleSet(generator);

			ruleSet.Description = new Description("Regeln");
			TheRuleSetBag.Description = new Description("Påsen");
			TheRuleSetBag.AddRuleSet(ruleSet);

			new WorkShiftRuleSetRepository(uow).Add(ruleSet);
			new RuleSetBagRepository(uow).Add(TheRuleSetBag);

			uow.Reassociate(user);
			user.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)).FirstOrDefault().RuleSetBag = TheRuleSetBag;
			Debug.Assert(uow.Contains(user));
		}
	}
}