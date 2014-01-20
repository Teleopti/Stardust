using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersister : IShiftTradeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestMapper _shiftTradeRequestMapper;
		private readonly IMappingEngine _autoMapper;
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSourceProvider;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IServiceBusEventPublisher _serviceBusSender;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeSetChecksum;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository, 
																		IShiftTradeRequestMapper shiftTradeRequestMapper, 
																		IMappingEngine autoMapper,
																		IServiceBusEventPublisher serviceBusSender,
																		INow now,
																		ICurrentDataSource dataSourceProvider,
																		ICurrentBusinessUnit businessUnitProvider,
																		ICurrentUnitOfWork currentUnitOfWork,
																		IShiftTradeRequestSetChecksum shiftTradeSetChecksum)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
			_autoMapper = autoMapper;
			_now = now;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
			_currentUnitOfWork = currentUnitOfWork;
			_serviceBusSender = serviceBusSender;
			_shiftTradeSetChecksum = shiftTradeSetChecksum;
		}

		public RequestViewModel Persist(ShiftTradeRequestForm form)
		{
			var personRequest = _shiftTradeRequestMapper.Map(form);
			_shiftTradeSetChecksum.SetChecksum(personRequest.Request);
			_personRequestRepository.Add(personRequest);

			createMessage(personRequest);

			return _autoMapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		private void createMessage(IPersonRequest personRequest)
		{
			if (_serviceBusSender.EnsureBus())
			{
				var message = new NewShiftTradeRequestCreated
					{
						BusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
						Datasource = _dataSourceProvider.Current().DataSourceName,
						PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
						Timestamp = _now.UtcDateTime()
					};
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _serviceBusSender.Publish(message));
			}
		}
	}
}