using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    public class AccountTime : Account
    {
        public AccountTime(DateOnly startDate)
            : base(startDate){}

        protected AccountTime(){}
    }
}