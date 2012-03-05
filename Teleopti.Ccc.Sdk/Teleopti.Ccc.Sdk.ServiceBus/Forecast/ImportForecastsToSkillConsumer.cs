using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ImportForecastsToSkillConsumer : ConsumerOf<ImportForecastsToSkill>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportForecastsToSkillConsumer));
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveForecastToSkillCommand _saveForecastToSkillCommand;
        private readonly ISkillRepository _skillRepository;
        private readonly IJobResultRepository _jobResultRepository;
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;

        public ImportForecastsToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory,ISaveForecastToSkillCommand saveForecastToSkillCommand, ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBroker messageBroker)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveForecastToSkillCommand = saveForecastToSkillCommand;
            _skillRepository = skillRepository;
            _jobResultRepository = jobResultRepository;
            _feedback = feedback;
            _messageBroker = messageBroker;
        }

        public void Consume(ImportForecastsToSkill message)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var skill = _skillRepository.Get(message.TargetSkillId);
                if (skill == null)
                {
                    Logger.Error("Skill not exists.");
                    return;
                }
                _saveForecastToSkillCommand.Execute(message.Date, skill, message.Forecasts);
            }
        }
    }
}
