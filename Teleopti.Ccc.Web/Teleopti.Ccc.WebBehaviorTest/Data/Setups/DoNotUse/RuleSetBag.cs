using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class RuleSetBag : IUserDataSetup
	{
		private readonly int _earliestStart;
		private readonly int _latestStart;
		private readonly int _earliestEnd;
		private readonly int _latestEnd;
		public Domain.Scheduling.ShiftCreator.RuleSetBag TheRuleSetBag;
		public string ShiftCategory { get; set; }

		public RuleSetBag(int earliestStart, int latestStart, int earliestEnd, int latestEnd)
		{
			_earliestStart = earliestStart;
			_latestStart = latestStart;
			_earliestEnd = earliestEnd;
			_latestEnd = latestEnd;
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var start = new TimePeriodWithSegment(new TimePeriod(_earliestStart, 0, _latestStart, 0), new TimeSpan(0, 15, 0));
			var end = new TimePeriodWithSegment(new TimePeriod(_earliestEnd, 0, _latestEnd, 0), new TimeSpan(0, 15, 0));
			TheRuleSetBag = new Domain.Scheduling.ShiftCreator.RuleSetBag();

			var activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
			var activityRepository = new ActivityRepository(unitOfWork, null, null);
			activityRepository.Add(activity);

			IShiftCategory shiftCategory;
			if (ShiftCategory != null)
			{
				shiftCategory = new ShiftCategoryRepository(unitOfWork).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
			}
			else
			{
				shiftCategory = ShiftCategoryFactory.CreateShiftCategory(RandomName.Make(), "Purple");
				var shiftCategoryRepository= new ShiftCategoryRepository(unitOfWork);
				shiftCategoryRepository.Add(shiftCategory);
			}

			var generator = new WorkShiftTemplateGenerator(activity, start, end, shiftCategory);
			var ruleSet = new WorkShiftRuleSet(generator);

			ruleSet.Description = new Description("Regeln");
			TheRuleSetBag.Description = new Description("Påsen");
			TheRuleSetBag.AddRuleSet(ruleSet);

			new WorkShiftRuleSetRepository(unitOfWork).Add(ruleSet);
			new RuleSetBagRepository(unitOfWork).Add(TheRuleSetBag);

			unitOfWork.Current().Reassociate(person);
			person.Period(new DateOnly(2014, 1, 1)).RuleSetBag = TheRuleSetBag;
		}
	}

}