using System;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;
		private readonly IEventPublisher _publisher;

		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IAbsenceRequestSynchronousValidator _absenceRequestSynchronousValidator;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
			IMappingEngine mapper,
			IEventPublisher publisher,
			ICurrentBusinessUnit businessUnitProvider,
			ICurrentDataSource currentDataSource,
			INow now,
			ICurrentUnitOfWork currentUnitOfWork, IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_publisher = publisher;
			_businessUnitProvider = businessUnitProvider;
			_currentDataSource = currentDataSource;
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
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
				var result = _absenceRequestSynchronousValidator.Validate(personRequest);
				if (!result.IsValid)
				{
					personRequest.Deny(null, result.ValidationErrors, _personRequestCheckAuthorization,
						PersonRequestDenyOption.AutoDeny);
				}
				_personRequestRepository.Add(personRequest);
			}

			if (_currentUnitOfWork != null && !personRequest.IsDenied)
			{
				var message = new NewAbsenceRequestCreatedEvent
				{
					LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					LogOnDatasource = _currentDataSource.Current().DataSourceName,
					PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime(),
					JobName = "Absence Request",
					UserName = personRequest.Person.Name.ToString()
				};
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _publisher.Publish(message));
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}