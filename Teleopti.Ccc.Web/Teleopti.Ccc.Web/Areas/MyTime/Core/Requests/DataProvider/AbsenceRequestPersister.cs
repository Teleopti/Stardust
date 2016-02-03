using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
											IMappingEngine mapper,
											IMessagePopulatingServiceBusSender serviceBusSender,
											ICurrentBusinessUnit businessUnitProvider,
											ICurrentDataSource currentDataSource,
											INow now,
											ICurrentUnitOfWork currentUnitOfWork)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_serviceBusSender = serviceBusSender;
			_businessUnitProvider = businessUnitProvider;
			_currentDataSource = currentDataSource;
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
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

			if (_currentUnitOfWork != null)
			{
				var message = new NewAbsenceRequestCreated
				{
					LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					LogOnDatasource = _currentDataSource.Current().DataSourceName,
					PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime()
				};
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _serviceBusSender.Send(message, false));
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}