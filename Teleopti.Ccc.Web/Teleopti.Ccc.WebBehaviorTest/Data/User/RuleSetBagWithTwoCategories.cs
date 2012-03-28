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
	public class RuleSetBagWithTwoCategories : IUserDataSetup
	{
		private readonly BaseShiftCategory _cat1;
		private readonly int _start1;
		private readonly int _end1;
		private readonly BaseShiftCategory _cat2;
		private readonly int _start2;
		private readonly int _end2;

		public Domain.Scheduling.ShiftCreator.RuleSetBag TheRuleSetBag;

		public RuleSetBagWithTwoCategories(BaseShiftCategory cat1, int start1, int end1, BaseShiftCategory cat2, int start2, int end2)
		{
			_cat1 = cat1;
			_start1 = start1;
			_end1 = end1;
			_cat2 = cat2;
			_start2 = start2;
			_end2 = end2;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var start1 = new TimePeriodWithSegment(new TimePeriod(_start1, 0, _start1, 0), new TimeSpan(0, 15, 0));
			var end1 = new TimePeriodWithSegment(new TimePeriod(_end1, 0, _end1, 0), new TimeSpan(0, 15, 0));
			var start2 = new TimePeriodWithSegment(new TimePeriod(_start2, 0, _start2, 0), new TimeSpan(0, 15, 0));
			var end2 = new TimePeriodWithSegment(new TimePeriod(_end2, 0, _end2, 0), new TimeSpan(0, 15, 0));
			TheRuleSetBag = new Domain.Scheduling.ShiftCreator.RuleSetBag();
			var generator1 = new WorkShiftTemplateGenerator(TestData.ActivityPhone, start1, end1, _cat1.ShiftCategory);
			var ruleSet1 = new WorkShiftRuleSet(generator1);
			var generator2 = new WorkShiftTemplateGenerator(TestData.ActivityPhone, start2, end2, _cat2.ShiftCategory);
			var ruleSet2 = new WorkShiftRuleSet(generator2);

			ruleSet1.Description = new Description("Regeln 1");
			ruleSet2.Description = new Description("Regeln 2");
			TheRuleSetBag.Description = new Description("Påsen");
			TheRuleSetBag.AddRuleSet(ruleSet1);
			TheRuleSetBag.AddRuleSet(ruleSet2);

			var workShiftRuleSetRepository = new WorkShiftRuleSetRepository(uow);
			workShiftRuleSetRepository.Add(ruleSet1);
			workShiftRuleSetRepository.Add(ruleSet2);
			new RuleSetBagRepository(uow).Add(TheRuleSetBag);

			uow.Reassociate(user);
			user.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)).FirstOrDefault().RuleSetBag = TheRuleSetBag;
			Debug.Assert(uow.Contains(user));
		}
	}
}