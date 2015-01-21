using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PasswordPolicyForUser
	{
#pragma warning disable 169
		private Guid id;
		private PersonInfo personInfo;
#pragma warning restore 169

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