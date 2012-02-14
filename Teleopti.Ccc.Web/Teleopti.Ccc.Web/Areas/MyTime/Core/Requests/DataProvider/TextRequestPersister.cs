using System;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
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
				throw new HttpException(404, "PersonRequest not found");
			_personRequestRepository.Remove(personRequest);
		}
	}
}