using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider {
	public class OvertimeRequestPersister : IOvertimeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly OvertimeRequestFormMapper _mapper;
		private readonly RequestsViewModelMapper _requestsMapper;
		private readonly IOvertimeRequestProcessor _overtimeRequestProcessor;

		public OvertimeRequestPersister(IPersonRequestRepository personRequestRepository, OvertimeRequestFormMapper mapper,
			RequestsViewModelMapper requestsMapper, IOvertimeRequestProcessor overtimeRequestProcessor, IToggleManager toggleManager)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_requestsMapper = requestsMapper;
			_overtimeRequestProcessor = overtimeRequestProcessor;
			_overtimeRequestProcessor.StaffingDataAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(toggleManager);
		}

		public RequestViewModel Persist(OvertimeRequestForm form)
		{
			IPersonRequest personRequest;
			if (form.Id.HasValue)
			{
				personRequest = _personRequestRepository.Get(form.Id.Value);

				if (personRequest == null)
				{
					throw new ApplicationException($"Person request with Id \"{form.Id}\" does not exist.");
				}

				if (!(personRequest.Request is IOvertimeRequest))
				{
					throw new ApplicationException($"Person request with Id \"{form.Id}\" is not an overtime request.");
				}
				personRequest = _mapper.Map(form, personRequest);

				_overtimeRequestProcessor.Process(personRequest);
			}
			else
			{
				personRequest = _mapper.Map(form, null);
				_personRequestRepository.Add(personRequest);

				_overtimeRequestProcessor.Process(personRequest);
			}

			return _requestsMapper.Map(personRequest);
		}
	}
}