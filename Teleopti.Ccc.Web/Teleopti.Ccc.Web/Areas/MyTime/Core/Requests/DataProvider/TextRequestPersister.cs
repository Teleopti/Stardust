using System;
using System.Web;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class TextRequestPersister : ITextRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly RequestsViewModelMapper _mapper;
		private readonly TextRequestFormMapper _formMapper;

		public TextRequestPersister(IPersonRequestRepository personRequestRepository, RequestsViewModelMapper mapper, TextRequestFormMapper formMapper)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_formMapper = formMapper;
		}

		public RequestViewModel Persist(TextRequestForm form)
		{
			IPersonRequest personRequest = null;
			if (form.EntityId.HasValue) {
				personRequest = _personRequestRepository.Find(form.EntityId.Value);
			}

			if (personRequest != null)
			{
				_formMapper.Map(form, personRequest);
			}
			else
			{
				personRequest = _formMapper.Map(form);
				_personRequestRepository.Add(personRequest);	
			}

			return _mapper.Map(personRequest);
		}

		public void Delete(Guid id)
		{
			var personRequest = _personRequestRepository.Find(id);
			if (personRequest == null)
				throw new RequestPersistException(404, "PersonRequest not found", Resources.Request);
			try
			{
				_personRequestRepository.Remove(personRequest);
				setExchangeOfferBack(personRequest);
			}
			catch (DataSourceException)
			{
				throw new RequestPersistException(404, Resources.RequestCannotUpdateDelete, Resources.Request);
			}
		}

		private void setExchangeOfferBack(IPersonRequest personRequest)
		{
			var shiftTradeRequest = personRequest.Request as IShiftTradeRequest;
			var offer = shiftTradeRequest?.Offer;
			if (offer != null && offer.Status == ShiftExchangeOfferStatus.PendingAdminApproval)
			{
				offer.Status = ShiftExchangeOfferStatus.Pending;
			}
		}
	}
	
	[Serializable]
	public class RequestPersistException : HttpException
	{
		public string Shortmessage { get; set; }

		public RequestPersistException(int httpCode, string message, string shortmessage)
			: base(httpCode, message)
		{
			Shortmessage = shortmessage;
		}
	}
}