using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PasswordPolicyForUser
	{
		private Guid id;
		private PersonInfo personInfo;

		public virtual DateTime LastPasswordChange { get; protected set; }
		public virtual DateTime InvalidAttemptsSequenceStart { get; protected set; }
		public virtual int InvalidAttempts { get; protected set; }
		public virtual bool IsLocked { get; protected set; }

		public virtual void Lock()
		{
			IsLocked = true;
		}
	}
}