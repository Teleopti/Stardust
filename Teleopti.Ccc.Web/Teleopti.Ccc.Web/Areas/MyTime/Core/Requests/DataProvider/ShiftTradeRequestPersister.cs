using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersister : IShiftTradeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestMapper _shiftTradeRequestMapper;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository, IShiftTradeRequestMapper shiftTradeRequestMapper)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
		}

		public void Persist(ShiftTradeRequestForm form)
		{
			var personRequest = _shiftTradeRequestMapper.Map(form);
			_personRequestRepository.Add(personRequest);
		}
	}
}