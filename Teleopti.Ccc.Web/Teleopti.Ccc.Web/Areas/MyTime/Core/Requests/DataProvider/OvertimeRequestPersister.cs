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
			var personRequest = _mapper.Map(form);
			_personRequestRepository.Add(personRequest);

			return _requestsMapper.Map(personRequest);
		}
	}
}