using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class UserLogonDetailConfigurable : IUserDataSetup
	{
		public int InvalidAttempts { get; set; }
		public int InvalidAttemptsStartXMinutesAgo { get; set; }
		public bool IsLocked { get; set; }
		public int LastPasswordChangeXDaysAgo { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var userDetail = new UserDetail(user);
			userDetail.SetInvalidAttempts(InvalidAttempts);
			userDetail.SetInvalidAttemptsSequenceStart(DateTime.Now.AddMinutes(-InvalidAttemptsStartXMinutesAgo));
			userDetail.SetLastPasswordChange(DateTime.Now.AddDays(-LastPasswordChangeXDaysAgo));
			if(IsLocked)
				userDetail.Lock();

			var userDetailRepository = new UserDetailRepository(uow);
			userDetailRepository.Add(userDetail);
		}
	}
}