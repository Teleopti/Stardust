﻿using System;
using System.Globalization;
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
        private readonly IJobResultRepository _jobResultRepository;
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;
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
                var jobResult = _jobResultRepository.Get(message.JobId);
                var targetSkill = _skillRepository.Get(message.TargetSkillId);
                _feedback.SetJobResult(jobResult, _messageBroker);
                _feedback.Info(string.Format(CultureInfo.InvariantCulture, "Open and split target skill {0} on {1}.",
                                                       targetSkill.Name, message.Date));
                _feedback.ReportProgress(1, string.Format(CultureInfo.InvariantCulture, "Open and split target skill {0} on {1}.",
                                                       targetSkill.Name, message.Date));
                using (unitOfWork.DisableFilter(QueryFilter.BusinessUnit))
                {
                    var openHours = new TimePeriod(message.StartOpenHour, message.EndOpenHour);
                    var dateOnly = new DateOnly(message.Date);
                    try { 
                    _command.Execute(targetSkill, new DateOnlyPeriod(dateOnly, dateOnly), new[] {openHours});
                    }
                    catch (Exception exception)
                    {
                        unitOfWork.Clear();
                        unitOfWork.Merge(jobResult);
                        _feedback.Error("An error occurred while running import.", exception);
                        _feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture, "An error occurred while running import."));
                        endProcessing(unitOfWork);
                        return;
                    }
                    _feedback.ReportProgress(1, string.Format(CultureInfo.InvariantCulture, "Open and split target skill {0} on {1} done.",
                                                           targetSkill.Name, message.Date));
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
                Timestamp = message.Timestamp,
                ImportMode = message.ImportMode
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
