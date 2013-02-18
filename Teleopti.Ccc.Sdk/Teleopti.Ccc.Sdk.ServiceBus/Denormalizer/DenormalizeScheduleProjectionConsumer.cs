﻿using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.RTA;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class DenormalizeScheduleProjectionConsumer : ConsumerOf<DenormalizeScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
	    private readonly IServiceBus _serviceBus;

	    public DenormalizeScheduleProjectionConsumer(IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
		    _serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeScheduleProjection message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				var person = _personRepository.Get(message.PersonId);

				if (message.SkipDelete)
				{
					_updateScheduleProjectionReadModel.SetInitialLoad(true);
				}
				_updateScheduleProjectionReadModel.Execute(scenario,period,person);
				unitOfWork.PersistAll();
			}
		}
	}
}
