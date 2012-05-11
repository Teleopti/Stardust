using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;
		private readonly IServiceBusSender _serviceBusSender;
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly ICurrentDataSourceProvider _dataSourceProvider;
		private readonly INow _now;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository, IMappingEngine mapper, IServiceBusSender serviceBusSender, ICurrentBusinessUnitProvider businessUnitProvider, ICurrentDataSourceProvider dataSourceProvider, INow now)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_serviceBusSender = serviceBusSender;
			_businessUnitProvider = businessUnitProvider;
			_dataSourceProvider = dataSourceProvider;
			_now = now;
		}

		public RequestViewModel Persist(AbsenceRequestForm form)
		{
			var personRequest = _mapper.Map<AbsenceRequestForm, IPersonRequest>(form);

			_personRequestRepository.Add(personRequest);

			if (_serviceBusSender.EnsureBus())
			{
				personRequest.SetNew();
				
				var message = new NewAbsenceRequestCreated()
				              	{
				              		BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.GetValueOrDefault(Guid.Empty),
				              		Datasource = _dataSourceProvider.CurrentDataSource().DataSourceName,
				              		PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
				              		Timestamp = _now.UtcTime
				              	};
				_serviceBusSender.NotifyServiceBus(message);
			}
			else
			{
				personRequest.Pending();
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}