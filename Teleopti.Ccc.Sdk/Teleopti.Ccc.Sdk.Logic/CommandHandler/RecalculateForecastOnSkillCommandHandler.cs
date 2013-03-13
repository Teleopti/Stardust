using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class RecalculateForecastOnSkillCommandHandler : IHandleCommand<RecalculateForecastOnSkillCollectionCommandDto>
	{
		private readonly IServiceBusSender _busSender;

		public RecalculateForecastOnSkillCommandHandler(IServiceBusSender busSender)
		{
			_busSender = busSender;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Handle(RecalculateForecastOnSkillCollectionCommandDto command)
		{
			// denna borde fångas
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the forecast.");
			}

        	var principal = TeleoptiPrincipal.Current;
			var person = ((IUnsafePerson)principal).Person;
			var identity = (ITeleoptiIdentity)principal.Identity;
            var message = new RecalculateForecastOnSkillMessageCollection
                {	
					MessageCollection = new Collection<RecalculateForecastOnSkillMessage>(),
					BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
					Datasource = identity.DataSource.Application.Name,
					Timestamp = DateTime.UtcNow,
					ScenarioId = command.ScenarioId,
					OwnerPersonId = person.Id.GetValueOrDefault()
				};
            foreach (var model in command.WorkloadOnSkillSelectionDtos)
            {
                message.MessageCollection.Add(
                    new RecalculateForecastOnSkillMessage
                        {
                            SkillId = model.SkillId,
                            WorkloadIds = new Collection<Guid>(model.WorkloadId)                            
                        });

            }
            _busSender.NotifyServiceBus(message);
			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}