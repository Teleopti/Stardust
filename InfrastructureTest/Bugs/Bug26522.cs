using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[DatabaseTest]
	public class Bug26522
	{
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;

		private IActivity activity;
		private IShiftCategory shiftCategory;
		private IWorkShiftRuleSet workShiftRuleSet;

		private void setup()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				activity = new Activity("for test");
				shiftCategory = new ShiftCategory("for test");
				var template = new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 30),
					new TimePeriodWithSegment(16, 0, 17, 0, 30), shiftCategory);
				workShiftRuleSet = new WorkShiftRuleSet(template) { Description = new Description("used in test") };

				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				workShiftRuleSet.AddAccessibilityDate(new DateOnly(2000, 1, 1));
				WorkShiftRuleSetRepository.Add(workShiftRuleSet);

				uow.PersistAll();
			}

		}

		[Test]
		public void ShouldPersistAddedAccessibilityDate()
		{
			setup();
			workShiftRuleSet.AddAccessibilityDate(DateOnly.Today);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = Infrastructure.Repositories.WorkShiftRuleSetRepository.DONT_USE_CTOR(uow);
				rep.Add(workShiftRuleSet);
				uow.PersistAll();
			}
		}

	}
}