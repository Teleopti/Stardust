using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public static class AbsenceRequestFormExtensions
	{
		public static AbsenceRequestModel ToModel(this AbsenceRequestForm form, IUserTimeZone timeZone, ILoggedOnUser loggedOnUser)
		{
			return new AbsenceRequestModel
			{
				Period = form.Period.Map(timeZone), AbsenceId = form.AbsenceId, FullDay = form.FullDay,
				Message = form.Message, Subject = form.Subject, PersonRequestId = form.EntityId,
				PersonId = loggedOnUser.CurrentUser().Id.GetValueOrDefault()
			};
		}
	}
}