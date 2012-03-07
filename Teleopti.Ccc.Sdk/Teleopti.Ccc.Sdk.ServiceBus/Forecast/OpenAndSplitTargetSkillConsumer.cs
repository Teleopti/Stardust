using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class OpenAndSplitTargetSkillConsumer : ConsumerOf<OpenAndSplitTargetSkill>
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IOpenAndSplitSkillCommand _command;
        private readonly ISkillRepository _skillRepository;
        private IJobResultRepository _jobResultRepository;
        private readonly IJobResultFeedback _feedback;
        private IMessageBroker _messageBroker;
        private readonly IServiceBus _serviceBus;

        public OpenAndSplitTargetSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory, IOpenAndSplitSkillCommand command, ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBroker messageBroker, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_command = command;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_serviceBus = serviceBus;
		}

        public void Consume(OpenAndSplitTargetSkill message)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.BusinessUnit))
                {
                    var targetSkill = _skillRepository.Get(message.TargetSkillId);
                    var openHours = new TimePeriod(message.StartOpenHour, message.EndOpenHour);
                    var dateOnly = new DateOnly(message.Date);
                    _command.Execute(targetSkill, new DateOnlyPeriod(dateOnly, dateOnly), new[] {openHours});
                    endProcessing(unitOfWork);
                }
            }
            _serviceBus.Send(new ImportForecastsToSkill
            {
                BusinessUnitId = message.BusinessUnitId,
                Datasource = message.Datasource,
                JobId = message.JobId,
                OwnerPersonId = message.OwnerPersonId,
                Date = message.Date,
                TargetSkillId = message.TargetSkillId,
                Forecasts = message.Forecasts,
                Timestamp = message.Timestamp
            });

            _unitOfWorkFactory = null;
        }

        private void endProcessing(IUnitOfWork unitOfWork)
        {
            unitOfWork.PersistAll();
            _feedback.Dispose();
        }
    }
}
