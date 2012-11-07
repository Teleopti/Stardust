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
		[Given(@"I have a shift with")]
		public void GivenIHaveAShiftWith(Table table)
		{
			var schedule = table.CreateInstance<ShiftConfigurable>();
			UserFactory.User().Setup(schedule);
		}
		
		[When(@"I have a shift with")]
		public void GivenThereIsAShiftWithWhen(Table table)
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