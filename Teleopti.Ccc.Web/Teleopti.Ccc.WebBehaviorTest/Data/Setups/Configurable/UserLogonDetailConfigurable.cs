using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class UserLogonDetailConfigurable : IUserDataSetup
	{
		public bool? IsLocked { get; set; }
		public int? LastPasswordChangeXDaysAgo { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var userDetail = new UserDetail(user);
			if (LastPasswordChangeXDaysAgo != null)
				userDetail.LastPasswordChange = DateTime.UtcNow.AddDays(-LastPasswordChangeXDaysAgo.Value);
			if (IsLocked != null && IsLocked.Value)
				userDetail.Lock();

			var userDetailRepository = new UserDetailRepository(uow);
			userDetailRepository.Add(userDetail);
		}
	}
}