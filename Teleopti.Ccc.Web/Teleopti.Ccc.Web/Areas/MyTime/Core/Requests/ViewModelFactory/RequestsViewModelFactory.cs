using System;
using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly IMappingEngine _mapper;

		public RequestsViewModelFactory(IPersonRequestProvider personRequestProvider, IMappingEngine mapper)
		{
			_personRequestProvider = personRequestProvider;
			_mapper = mapper;
		}

		public RequestsViewModel CreatePageViewModel()
		{
			return new RequestsViewModel();
		}

		public IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging)
		{
			var requests = _personRequestProvider.RetrieveTextRequests(paging);
			return _mapper.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(requests);
		}

		public RequestViewModel CreateRequestViewModel(Guid id)
		{
			var request = _personRequestProvider.RetrieveRequest(id);
			return _mapper.Map<IPersonRequest, RequestViewModel>(request);
		}
	}
}