using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftStepDefinitions
	{
		[Given(@"(.*) have a shift with")]
		public void GivenIHaveAShiftWith(string userName, Table table)
		{
			var schedule = table.CreateInstance<ShiftConfigurable>();
			UserFactory.User(userName).Setup(schedule);
		}

		[When(@"I am assigned this shift with")]
		public void WhenIAmAssignedThisShiftWith(Table table)
		{
			var schedule = table.CreateInstance<ShiftConfigurable>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var user = UserFactory.User().Person;
				schedule.Apply(uow, user, user.PermissionInformation.Culture());
				uow.PersistAll();
			}
		}
	}
}