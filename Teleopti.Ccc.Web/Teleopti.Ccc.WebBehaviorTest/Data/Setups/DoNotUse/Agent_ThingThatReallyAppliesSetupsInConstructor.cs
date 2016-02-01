using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class Agent_ThingThatReallyAppliesSetupsInConstructor : IUserSetup, IUserRoleSetup
	{
		public Agent_ThingThatReallyAppliesSetupsInConstructor()
		{
			var role = new agentRole();
			if (!DataMaker.Data().HasSetup<agentRole>())
				DataMaker.Data().Apply(role);
			if (!DataMaker.Data().HasSetup<RoleForUser>())
				DataMaker.Me().Apply(new RoleForUser { Name = role.Name });
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		}

		private  class agentRole : RoleConfigurable
		{
			public agentRole()
			{
				Name = "Agent";
				AccessToStudentAvailability = true;
			}
		}
	}
	
}