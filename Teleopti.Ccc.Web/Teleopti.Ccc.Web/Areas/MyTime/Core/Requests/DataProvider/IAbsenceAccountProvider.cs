using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
    public interface IAbsenceAccountProvider
    {
        IAccount GetPersonAccount(IAbsence absence, DateOnly date);
    }
}