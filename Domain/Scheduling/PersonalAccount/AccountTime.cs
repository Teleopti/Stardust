using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    public class AccountTime : Account
    {
        public AccountTime(DateOnly startDate)
            : base(startDate){}

        protected AccountTime(){}
    }
}