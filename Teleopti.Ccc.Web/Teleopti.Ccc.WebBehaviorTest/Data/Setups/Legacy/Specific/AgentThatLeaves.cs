using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AgentThatLeaves : IUserSetup, IUserRoleSetup
	{
		private readonly DateTime _leavingDate;

		public AgentThatLeaves(DateTime leavingDate)
		{
			_leavingDate = leavingDate;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRole);
			user.TerminatePerson(new DateOnly(_leavingDate), new PersonAccountUpdaterDummy());
		}
	}
}