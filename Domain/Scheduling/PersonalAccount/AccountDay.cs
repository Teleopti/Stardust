using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    public class AccountDay : Account
    {
        public AccountDay(DateOnly startDate)
            : base(startDate){}

        protected AccountDay(){}

    }
}