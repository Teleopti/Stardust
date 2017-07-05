using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider {
	public class OvertimeRequestPersister : IOvertimeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly OvertimeRequestFormMapper _mapper;
		private readonly RequestsViewModelMapper _requestsMapper;

		public OvertimeRequestPersister(IPersonRequestRepository personRequestRepository, OvertimeRequestFormMapper mapper,
			RequestsViewModelMapper requestsMapper)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_requestsMapper = requestsMapper;
		}

		public RequestViewModel Persist(OvertimeRequestForm form)
		{
			var isCreatingNew = true;
			IPersonRequest personRequest = null;
			if (form.Id.HasValue)
			{
				isCreatingNew = false;
				personRequest = _personRequestRepository.Get(form.Id.Value);

				if (personRequest == null)
				{
					throw new ApplicationException($"Person request with Id \"{form.Id}\" does not exist.");
				}

				if (!(personRequest.Request is IOvertimeRequest))
				{
					throw new ApplicationException($"Person request with Id \"{form.Id}\" is not an overtime request.");
				}
			}

			personRequest = _mapper.Map(form, personRequest);
			if (isCreatingNew)
			{
				_personRequestRepository.Add(personRequest);
				process(personRequest);
			}

			return _requestsMapper.Map(personRequest);
		}

		private void process(IPersonRequest personRequest)
		{
			personRequest.Pending();
		}
	}
}