using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	public class Bug26522 : DatabaseTestWithoutTransaction
	{
		private IActivity activity;
		private IShiftCategory shiftCategory;
		private IWorkShiftRuleSet workShiftRuleSet;

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			activity = new Activity("for test");
			shiftCategory = new ShiftCategory("for test"); 
			var template = new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 30),
				new TimePeriodWithSegment(16, 0, 17, 0, 30), shiftCategory);
			workShiftRuleSet = new WorkShiftRuleSet(template) {Description = new Description("used in test")};

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(shiftCategory);
			workShiftRuleSet.AddAccessibilityDate(new DateTime(2000,1,1,0,0,0,DateTimeKind.Utc));
			PersistAndRemoveFromUnitOfWork(workShiftRuleSet);

			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldPersistAddedAccessibilityDate()
		{
			workShiftRuleSet.AddAccessibilityDate(DateTime.UtcNow);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new WorkShiftRuleSetRepository(uow);
				rep.Add(workShiftRuleSet);
				uow.PersistAll();
			}
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new Repository(uow);
				rep.Remove(workShiftRuleSet);
				rep.Remove(shiftCategory);
				rep.Remove(activity);
				uow.PersistAll();
			}
		}
	}
}