using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class DenormalizeGroupingMessageConsumer : ConsumerOf<DenormalizeGroupingMessage>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		
		private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;

        public DenormalizeGroupingMessageConsumer(IUnitOfWorkFactory unitOfWorkFactory, IGroupingReadOnlyRepository groupingReadOnlyRepository,IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeGroupingMessage message)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				//var scenario = _scenarioRepository.Get(message.ScenarioId);
				//if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				
				if (message.SkipDelete)
				{
					_updateScheduleProjectionReadModel.SetSkipDelete(true);
				}
				_updateScheduleProjectionReadModel.Execute(scenario,period,person);
			}
		}
	}
}