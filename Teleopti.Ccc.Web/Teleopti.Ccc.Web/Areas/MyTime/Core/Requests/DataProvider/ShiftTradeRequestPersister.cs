﻿using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
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
		private readonly IDataSourceProvider _dataSourceProvider;
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly IServiceBusSender _serviceBusSender;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeSetChecksum;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository, 
																		IShiftTradeRequestMapper shiftTradeRequestMapper, 
																		IMappingEngine autoMapper,
																		IServiceBusSender serviceBusSender,
																		INow now,
																		IDataSourceProvider dataSourceProvider,
																		ICurrentBusinessUnitProvider businessUnitProvider,
																		IUnitOfWorkFactory uowFactory,
																		IShiftTradeRequestSetChecksum shiftTradeSetChecksum)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
			_autoMapper = autoMapper;
			_now = now;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
			_serviceBusSender = serviceBusSender;
			_uowFactory = uowFactory;
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
						BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.GetValueOrDefault(Guid.Empty),
						Datasource = _dataSourceProvider.CurrentDataSource().DataSourceName,
						PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
						Timestamp = _now.UtcDateTime()
					};
				_uowFactory.CurrentUnitOfWork().AfterSuccessfulTx(() => _serviceBusSender.NotifyServiceBus(message));
			}
		}
	}
}