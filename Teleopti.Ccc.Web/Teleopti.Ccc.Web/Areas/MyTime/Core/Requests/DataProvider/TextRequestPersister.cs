using System;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class TextRequestPersister : ITextRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;

		public TextRequestPersister(IPersonRequestRepository personRequestRepository, IMappingEngine mapper)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
		}

		public RequestViewModel Persist(TextRequestForm form)
		{
			IPersonRequest personRequest = null;
			if (form.EntityId.HasValue) {
				personRequest = _personRequestRepository.Find(form.EntityId.Value);
			}

			if (personRequest != null)
			{
				_mapper.Map(form, personRequest);
			}
			else
			{
				personRequest = _mapper.Map<TextRequestForm, IPersonRequest>(form);
				_personRequestRepository.Add(personRequest);	
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
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
			if (shiftTradeRequest == null) return;
			var offer = shiftTradeRequest.Offer;
			if (offer != null && offer.Status == ShiftExchangeOfferStatus.PendingAdminApproval)
			{
				offer.Status = ShiftExchangeOfferStatus.Pending;
			}
		}
	}



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly"), Serializable]
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