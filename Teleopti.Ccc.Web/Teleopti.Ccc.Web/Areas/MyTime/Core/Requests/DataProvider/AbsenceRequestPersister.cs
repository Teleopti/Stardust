using System;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;
		private readonly IServiceBusSender _serviceBusSender;
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;
		private readonly IUnitOfWorkFactory _uowFactory;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository, 
											IMappingEngine mapper, 
											IServiceBusSender serviceBusSender, 
											ICurrentBusinessUnitProvider businessUnitProvider, 
											ICurrentDataSource currentDataSource, 
											INow now,
											IUnitOfWorkFactory uowFactory)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_serviceBusSender = serviceBusSender;
			_businessUnitProvider = businessUnitProvider;
			_currentDataSource = currentDataSource;
			_now = now;
			_uowFactory = uowFactory;
		}

		public RequestViewModel Persist(AbsenceRequestForm form)
		{
			IPersonRequest personRequest = null;
			if (form.EntityId.HasValue)
			{
				personRequest = _personRequestRepository.Find(form.EntityId.Value);
			}
			if (personRequest != null)
			{
				try
				{
					_mapper.Map(form, personRequest);
				}
				catch (AutoMapperMappingException e)
				{
					if (e.InnerException is InvalidOperationException)
					{
						// this catch is intent to catch InvalidOperationException throw from PersonRequest#CheckIfEditable
						throw e.InnerException;
					}
					throw;
				}
			}
			else
			{
				personRequest = _mapper.Map<AbsenceRequestForm, IPersonRequest>(form);
				_personRequestRepository.Add(personRequest);
			}

			if (_serviceBusSender.EnsureBus())
			{
				var message = new NewAbsenceRequestCreated
				              	{
				              		BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.GetValueOrDefault(Guid.Empty),
				              		Datasource = _currentDataSource.Current().DataSourceName,
				              		PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
				              		Timestamp = _now.UtcDateTime()
				              	};
				_uowFactory.CurrentUnitOfWork().AfterSuccessfulTx(() => _serviceBusSender.Send(message));
			}
			else
			{
				personRequest.Pending();
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}