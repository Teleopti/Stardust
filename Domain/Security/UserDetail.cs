using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security
{
    public class UserDetail : SimpleAggregateRoot, IUserDetail
    {
        private readonly IPerson _person;
		public DateTime LastPasswordChange { get; set; }
		public int InvalidAttempts { get; set; }
        private bool _isLocked;
		public DateTime InvalidAttemptsSequenceStart { get; set; }

        protected UserDetail()
        {
        }

        public UserDetail(IPerson person) : this()
        {
            _person = person;
			LastPasswordChange = DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
        }

        public virtual bool IsLocked
        {
            get {
                return _isLocked;
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

	    public virtual void RegisterInvalidAttempt(IPasswordPolicy passwordPolicy)
        {
            DateTime utcNow = DateTime.UtcNow;
			if (utcNow > InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow) ||
                InvalidAttempts == 0)
            {
                //Start of new sequence
				InvalidAttemptsSequenceStart = utcNow;
				InvalidAttempts = 0;
            }
			InvalidAttempts++;
        }

        public virtual void RegisterPasswordChange()
        {
			LastPasswordChange = DateTime.UtcNow;
			InvalidAttempts = 0;
            _isLocked = false;
        }
    }
}