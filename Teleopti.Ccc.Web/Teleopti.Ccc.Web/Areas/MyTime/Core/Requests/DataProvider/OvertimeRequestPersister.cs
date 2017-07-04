using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
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
		private readonly IQueuedOvertimeRequestRepository _queuedOvertimeRequestRepository;

		public OvertimeRequestPersister(IPersonRequestRepository personRequestRepository, OvertimeRequestFormMapper mapper,
			RequestsViewModelMapper requestsMapper, IQueuedOvertimeRequestRepository queuedOvertimeRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_requestsMapper = requestsMapper;
			_queuedOvertimeRequestRepository = queuedOvertimeRequestRepository;
		}

		public RequestViewModel Persist(OvertimeRequestForm form)
		{
			var isCreatingNew = true;
			IPersonRequest personRequest = null;
			if (form.Id.HasValue)
			{
				isCreatingNew = false;
				personRequest = _personRequestRepository.Get(form.Id.Value);
				if (!(personRequest.Request is IOvertimeRequest))
				{
					throw new ApplicationException($"Request with Id {form.Id} is not an overtime request.");
				}
			}

			personRequest = _mapper.Map(form, personRequest);
			if (isCreatingNew)
			{
				_personRequestRepository.Add(personRequest);
				queueOvertimeRequest(personRequest);
			}

			return _requestsMapper.Map(personRequest);
		}

		private void queueOvertimeRequest(IPersonRequest personRequest)
		{
			personRequest.Pending();
			var queuedOvertimeRequest = new QueuedOvertimeRequest()
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				Created = personRequest.CreatedOn.GetValueOrDefault(),
				StartDateTime = personRequest.Request.Period.StartDateTime,
				EndDateTime = personRequest.Request.Period.EndDateTime
			};
			_queuedOvertimeRequestRepository.Add(queuedOvertimeRequest);
		}
	}
}