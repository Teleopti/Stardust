using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security
{
	//TODO: tenant Can be removed when removing old schema
	public class UserDetail : SimpleAggregateRoot, IUserDetail
	{
		private readonly IPerson _person;
		private DateTime _lastPasswordChange;
		private int _invalidAttempts;
		private bool _isLocked;
		private DateTime _invalidAttemptsSequenceStart;

		protected UserDetail()
		{
		}

		public UserDetail(IPerson person) : this()
		{
			_person = person;
			_lastPasswordChange = DateTime.UtcNow;
			_invalidAttemptsSequenceStart = DateTime.UtcNow;
		}

		public virtual DateTime LastPasswordChange
		{
			get {
				return _lastPasswordChange;
			}
			set { _lastPasswordChange = value; }
		}

		public virtual int InvalidAttempts
		{
			get {
				return _invalidAttempts;
			}
			set { _invalidAttempts = value; }
		}

		public virtual bool IsLocked
		{
			get {
				return _isLocked;
			}
		}

		public virtual DateTime InvalidAttemptsSequenceStart
		{
			get {
				return _invalidAttemptsSequenceStart;
			}
			set { _invalidAttemptsSequenceStart = value; }
		}

		public virtual IPerson Person
		{
			get {
				return _person;
			}
		}

		public virtual void Lock()
		{
			_isLocked = true;
		}

		public virtual void RegisterInvalidAttempt(IPasswordPolicy passwordPolicy)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (utcNow>_invalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow) ||
				_invalidAttempts == 0)
			{
				//Start of new sequence
				_invalidAttemptsSequenceStart = utcNow;
				_invalidAttempts = 0;
			}
			_invalidAttempts++;
		}

		public virtual void RegisterPasswordChange()
		{
			_lastPasswordChange = DateTime.UtcNow;
			_invalidAttempts = 0;
			_isLocked = false;
		}
	}
}