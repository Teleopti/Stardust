using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
    public interface IAbsenceAccountProvider
    {
        IAccount GetPersonAccount(IAbsence absence, DateOnly date);
    }
}