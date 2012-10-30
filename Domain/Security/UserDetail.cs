using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security
{
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
        }

        public virtual int InvalidAttempts
        {
            get {
                return _invalidAttempts;
            }
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


		/// <summary>
		/// Sets the invalid attempts.
		/// </summary>
		/// <param name="attempts">The attempts.</param>
		/// <remarks>
		/// Only for test
		/// </remarks>
		public virtual void SetInvalidAttempts(int attempts)
		{
			_invalidAttempts = attempts;
		}

		/// <summary>
		/// Sets the invalid attempts sequence start.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <remarks>
		/// Only for test
		/// </remarks>
		public virtual void SetInvalidAttemptsSequenceStart(DateTime startTime)
		{
			_invalidAttemptsSequenceStart = startTime;
		}

		/// <summary>
		/// Sets the last password change.
		/// </summary>
		/// <param name="lastPasswordChange">The last password change.</param>
		/// <remarks>
		/// Only for test
		/// </remarks>
		public virtual void SetLastPasswordChange(DateTime lastPasswordChange)
		{
			_lastPasswordChange = lastPasswordChange;
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