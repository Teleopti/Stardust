using System.Collections.Generic;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeRequestMapper(IPersonRepository personRepository, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
		}

		public IPersonRequest Map(ShiftTradeRequestForm form)
		{
			var personTo = _personRepository.Get(form.PersonToId);
			var ret = new PersonRequest(personTo);
			ret.Subject = form.Subject;
			ret.TrySetMessage(form.Message);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_loggedOnUser.CurrentUser(), personTo, form.Date, form.Date);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			ret.Request = shiftTradeRequest;
			return ret;
		}
	}
}