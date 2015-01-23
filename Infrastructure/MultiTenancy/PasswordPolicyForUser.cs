using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PasswordPolicyForUser
	{
#pragma warning disable 169
		private Guid id;
#pragma warning restore 169

		public PasswordPolicyForUser(PersonInfo personInfo)
		{
			PersonInfo = personInfo;
			LastPasswordChange=DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
		}
		protected PasswordPolicyForUser() { }

		public virtual DateTime LastPasswordChange { get; protected set; }
		public virtual DateTime InvalidAttemptsSequenceStart { get; protected set; }
		public virtual int InvalidAttempts { get; protected set; }
		public virtual bool IsLocked { get; protected set; }
		public virtual PersonInfo PersonInfo { get; protected set; }

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public bool ValidPassword(string encryptedPassword)
		{
			return personInfo.Password.Equals(encryptedPassword);
		}
	}
}