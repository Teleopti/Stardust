using System;
using System.Threading;
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
		private readonly IDataSourceProvider _dataSourceProvider;
		private readonly INow _now;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository, IMappingEngine mapper, IServiceBusSender serviceBusSender, ICurrentBusinessUnitProvider businessUnitProvider, IDataSourceProvider dataSourceProvider, INow now)
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
				              		Datasource = _dataSourceProvider.CurrentDataSource().DataSourceName,
				              		PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
				              		Timestamp = _now.UtcTime
				              	};
				var state = new TimerState {Message = message};
				state.Timer = new Timer(notifyServiceBusAfterASmallDelay,state,TimeSpan.FromMilliseconds(50),TimeSpan.FromMilliseconds(-1));
			}
			else
			{
				personRequest.Pending();
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		private void notifyServiceBusAfterASmallDelay(object state)
		{
			var timerState = (TimerState)state;
			_serviceBusSender.NotifyServiceBus(timerState.Message);
			timerState.DisposeTimer();
		}

		private class TimerState
		{
			public Timer Timer { get; set; }
			public NewAbsenceRequestCreated Message { get; set; }
			
			public void DisposeTimer()
			{
				if (Timer!=null)
				{
					Timer.Dispose();
					Timer = null;
				}
			}
		}
	}
}