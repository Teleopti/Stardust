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
		public bool? IsLocked { get; set; }
		public DateTime? LastPasswordChange { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var userDetail = new UserDetail(user);
			userDetail.InvalidAttempts = InvalidAttempts;
			userDetail.InvalidAttemptsSequenceStart = DateTime.Now.AddMinutes(-InvalidAttemptsStartXMinutesAgo);
			if (LastPasswordChange != null)
				userDetail.LastPasswordChange = LastPasswordChange.Value;
			if (IsLocked != null && IsLocked.Value)
				userDetail.Lock();

			var userDetailRepository = new UserDetailRepository(uow);
			userDetailRepository.Add(userDetail);
		}
	}
}