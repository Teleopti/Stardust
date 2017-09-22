using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftExchangeOfferPersister : IShiftExchangeOfferPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly RequestsViewModelMapper _mapper;
		private readonly IShiftExchangeOfferMapper _shiftExchangeOfferMapper;

		public ShiftExchangeOfferPersister(IPersonRequestRepository personRequestRepository,
														RequestsViewModelMapper mapper, IShiftExchangeOfferMapper shiftExchangeOfferMapper)
		{
			_shiftExchangeOfferMapper = shiftExchangeOfferMapper;
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
		}

		public RequestViewModel Persist(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status)
		{

			IPersonRequest personRequest = null;
			if (form.Id.HasValue)
			{
				personRequest = _personRequestRepository.Find(form.Id.Value);
			}

			if (personRequest != null)
			{
				_shiftExchangeOfferMapper.Map(form, personRequest);
			}
			else
			{
				personRequest = _shiftExchangeOfferMapper.Map(form, status);
				_personRequestRepository.Add (personRequest);
			}
			return _mapper.Map(personRequest);
		}
	}
}