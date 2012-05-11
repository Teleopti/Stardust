using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IAbsenceRequestPersister
	{
		RequestViewModel Persist(AbsenceRequestForm form);
	}
}