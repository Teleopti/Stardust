using System;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class Agent : IUserSetup, IUserRoleSetup
	{
		public Agent()
		{
			var role = new AgentRole();
			if (!DataMaker.Data().HasSetup<AgentRole>())
				DataMaker.Data().Apply(role);
			if (!DataMaker.Data().HasSetup<RoleForUser>())
				DataMaker.Me().Apply(new RoleForUser { Name = role.Name });
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		}
	}

	public class StudentAgent : Agent
	{
	}

	public class AgentThatLeaves : Agent
	{
		public AgentThatLeaves(DateTime date)
		{
			DataMaker.Me().Apply(new LeavingDateForUser {LeavingDate = date});
		}

	}

	public class AgentRole : RoleConfigurable
	{
		public AgentRole()
		{
			Name = "Agent";
			AccessToStudentAvailability = true;
		}
	}
}