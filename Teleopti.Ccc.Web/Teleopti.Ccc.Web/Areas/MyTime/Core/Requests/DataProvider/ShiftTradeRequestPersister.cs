using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersister : IShiftTradeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestMapper _shiftTradeRequestMapper;
		private readonly IMappingEngine _autoMapper;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository, IShiftTradeRequestMapper shiftTradeRequestMapper, IMappingEngine autoMapper)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
			_autoMapper = autoMapper;
		}

		public RequestViewModel Persist(ShiftTradeRequestForm form)
		{
			var personRequest = _shiftTradeRequestMapper.Map(form);
			_personRequestRepository.Add(personRequest);
			return _autoMapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}