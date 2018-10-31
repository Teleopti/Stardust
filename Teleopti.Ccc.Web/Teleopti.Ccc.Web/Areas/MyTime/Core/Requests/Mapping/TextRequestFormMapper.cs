using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class TextRequestFormMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTimeZone _userTimeZone;

		public TextRequestFormMapper(ILoggedOnUser loggedOnUser, IUserTimeZone userTimeZone)
		{
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
		}

		public IPersonRequest Map(TextRequestForm source, IPersonRequest destination = null)
		{
			if (destination == null)
			{
				var person = _loggedOnUser.CurrentUser();
				destination = new PersonRequest(person);
				destination.Pending();
			}

			DateTimePeriod period;
			if (source.FullDay)
			{
				var sourceTimeZone = _userTimeZone.TimeZone();
				var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), sourceTimeZone);
				var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), sourceTimeZone);
				period = new DateTimePeriod(startTime, endTime);
			}
			else
			{
				period = source.Period.Map(_userTimeZone);
			}

			destination.TrySetMessage(source.Message ?? "");

			var textRequest = new TextRequest(period);
			destination.Request = textRequest;

			if (source.EntityId != null)
				destination.SetId(source.EntityId);

			destination.Subject = source.Subject;
			
			return destination;
		}
	}
}