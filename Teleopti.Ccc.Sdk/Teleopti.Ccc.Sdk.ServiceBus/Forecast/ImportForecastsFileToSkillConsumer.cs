using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ImportForecastsFileToSkillConsumer : ConsumerOf<ImportForecastsFileToSkill>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportForecastsFileToSkillConsumer));
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISkillRepository _skillRepository;
        private readonly IJobResultRepository _jobResultRepository;
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;
        private readonly IServiceBus _serviceBus;

        public ImportForecastsFileToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory,ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBroker messageBroker, IServiceBus serviceBus)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _skillRepository = skillRepository;
            _jobResultRepository = jobResultRepository;
            _feedback = feedback;
            _messageBroker = messageBroker;
            _serviceBus = serviceBus;
        }

        public void Consume(ImportForecastsFileToSkill message)
        {
            ForecastsAnalyzeCommandResult commandResult;
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                //var jobResult = _jobResultRepository.Get(message.JobId);
                //LazyLoadingManager.Initialize(jobResult.Details);

                //if (jobResult.FinishedOk)
                //{
                //    Logger.InfoFormat("The forecasts import with job id {0} was already processed.", message.JobId);
                //    return;
                //}

                //_feedback.SetJobResult(jobResult, _messageBroker);
                //_feedback.ReportProgress(1, "Starting import...");

                var targetSkill = _skillRepository.Get(message.TargetSkillId);
                if (targetSkill == null)
                {
                    //_feedback.Error("Invalid skill id in the message.");
                    endProcessing(unitOfWork);
                    return;
                }
                var timeZone = targetSkill.TimeZone;
                
                using (var stream = new StreamReader(@"c:\temp\test.csv"))
                {
                    var reader = new CsvFileReader(stream);
                    var provider = new ForecastsFileContentProvider(reader, timeZone);
                    provider.LoadContent();
                    var analyzeCommand = new ForecastsAnalyzeCommand(provider.Forecasts);
                    commandResult = analyzeCommand.Execute();
                    if (!commandResult.Succeeded)
                    {
                        _feedback.Error(commandResult.ErrorMessage);
                        endProcessing(unitOfWork);
                        return;
                    }
                }
            }

            var listOfMessages = new List<OpenAndSplitTargetSkill>();
            foreach (var date in commandResult.Period.DayCollection())
            {
                var openHours = commandResult.WorkloadDayOpenHours.Get(date);
                listOfMessages.Add(new OpenAndSplitTargetSkill
                {
                    BusinessUnitId = message.BusinessUnitId,
                    Datasource = message.Datasource,
                    JobId = message.JobId,
                    OwnerPersonId = message.OwnerPersonId,
                    Date = date,
                    Timestamp = message.Timestamp,
                    TargetSkillId = message.TargetSkillId,
                    StartOpenHour = openHours.StartTime,
                    EndOpenHour = openHours.EndTime,
                    Forecasts = commandResult.ForecastFileDictionary.Get(date)
                });
            }
            listOfMessages.ForEach(m => _serviceBus.Send(m));
            _unitOfWorkFactory = null;
        }

        private void endProcessing(IUnitOfWork unitOfWork)
        {
            unitOfWork.PersistAll();
            _feedback.Dispose();
        }
    }

    public class ForecastsFileContentProvider
    {
        private readonly CsvFileReader _reader;
        private readonly ICccTimeZoneInfo _timeZone;
        private readonly ICollection<IForecastsFileRow> _forecasts = new List<IForecastsFileRow>();

        public ICollection<IForecastsFileRow> Forecasts
        {
            get { return _forecasts; }
        }

        public ForecastsFileContentProvider(CsvFileReader reader, ICccTimeZoneInfo timeZone)
        {
            _reader = reader;
            _timeZone = timeZone;
        }

        public void LoadContent()
        {
            var row = new CsvFileRow();
            var validators = new List<IForecastsFileValidator>
                                 {
                                     new ForecastsFileSkillNameValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileIntegerValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator()
                                 };

            while (_reader.ReadNextRow(row))
            {
                if (row.Count < 6 || row.Count > 7)
                {
                    throw new ValidationException("There are more or less columns than expected.");
                }
                for (var i = 0; i < row.Count; i++)
                {
                    if (!validators[i].Validate(row[i]))
                        throw new ValidationException(validators[i].ErrorMessage);
                }
                _forecasts.Add(ForecastsFileRowCreator.Create(row, _timeZone));
                row.Clear();
            }
        }
    }

    public class ForecastsAnalyzeCommand : IForecastsAnalyzeCommand
    {
        private readonly IEnumerable<IForecastsFileRow> _forecasts;

        public ForecastsAnalyzeCommand(IEnumerable<IForecastsFileRow> forecasts)
        {
            _forecasts = forecasts;
        }

        public ForecastsAnalyzeCommandResult Execute()
        {
            var result = new ForecastsAnalyzeCommandResult {ForecastFileDictionary = new ForecastFileDictionary()};
            var firstRow = _forecasts.First();
            var intervalLengthTicks = firstRow.LocalDateTimeTo.Subtract(firstRow.LocalDateTimeFrom).Ticks;
            var skillName = firstRow.SkillName;
            var startDateTime = DateTime.MaxValue;
            var endDateTime = DateTime.MinValue;
            var workloadDayOpenHours = new WorkloadDayOpenHoursDictionary();
            foreach (var forecastsRow in _forecasts)
            {
                if (!forecastsRow.SkillName.Equals(skillName))
                {
                    result.ErrorMessage = "There exists multiple skill names in the file.";
                    break;
                }
                if (forecastsRow.LocalDateTimeTo.Subtract(forecastsRow.LocalDateTimeFrom).Ticks != intervalLengthTicks)
                {
                    result.ErrorMessage = "Intervals do not have the same length.";
                    break;
                }
                if (forecastsRow.LocalDateTimeFrom < startDateTime)
                    startDateTime = forecastsRow.LocalDateTimeFrom;
                if (forecastsRow.LocalDateTimeTo > endDateTime)
                    endDateTime = forecastsRow.LocalDateTimeTo;

                workloadDayOpenHours.Add(new DateOnly(forecastsRow.LocalDateTimeFrom),
                                         new TimePeriod(forecastsRow.LocalDateTimeFrom.TimeOfDay,
                                                        forecastsRow.LocalDateTimeTo.TimeOfDay));

                result.ForecastFileDictionary.Add(new DateOnly(forecastsRow.LocalDateTimeFrom), forecastsRow);
            }
            result.Period = new DateOnlyPeriod(new DateOnly(startDateTime), new DateOnly(endDateTime));
            result.WorkloadDayOpenHours = workloadDayOpenHours;
            result.IntervalLengthTicks = intervalLengthTicks;
            result.SkillName = firstRow.SkillName;
            return result;
        }
    }

    public interface IForecastsAnalyzeCommand
    {
        ForecastsAnalyzeCommandResult Execute();
    }

    public class ForecastsAnalyzeCommandResult
    {
        public string ErrorMessage { get; set; }
        public bool Succeeded { get { return string.IsNullOrEmpty(ErrorMessage); } }
        public string SkillName { get; set; }
        public long IntervalLengthTicks { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IForecastFileDictionary ForecastFileDictionary { get; set; }
        public IWorkloadDayOpenHoursDictionary WorkloadDayOpenHours { get; set; }
    }

    public interface IWorkloadDayOpenHoursDictionary
    {
        void Add(DateOnly dateOnly, TimePeriod openHours);
        TimePeriod Get(DateOnly dateOnly);
    }

    public class WorkloadDayOpenHoursDictionary : IWorkloadDayOpenHoursDictionary
    {
        private readonly IDictionary<DateOnly, TimePeriod> _workloadDayOpenHours =
            new Dictionary<DateOnly, TimePeriod>();

        public void Add(DateOnly dateOnly, TimePeriod openHours)
        {
            TimePeriod existingOpenHours;
            if (_workloadDayOpenHours.TryGetValue(dateOnly, out existingOpenHours))
            {
                TimeSpan mergedStartTime;
                TimeSpan mergedEndTime;
                if (openHours.StartTime.Subtract(existingOpenHours.StartTime) < TimeSpan.Zero)
                {
                    mergedStartTime = openHours.StartTime;
                    mergedEndTime = existingOpenHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
                if (openHours.EndTime.Subtract(existingOpenHours.EndTime) > TimeSpan.Zero)
                {
                    mergedStartTime = existingOpenHours.StartTime;
                    mergedEndTime = openHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
            }
            else
                _workloadDayOpenHours.Add(dateOnly, openHours);
        }

        public TimePeriod Get(DateOnly dateOnly)
        {
            TimePeriod openHours;
            _workloadDayOpenHours.TryGetValue(dateOnly, out openHours);
            return openHours;
        }
    }


    public interface IForecastFileDictionary
    {
        void Add(DateOnly date, IForecastsFileRow forecasts);
        ICollection<IForecastsFileRow> Get(DateOnly date);
    }

    public class ForecastFileDictionary : IForecastFileDictionary
    {
        private readonly IDictionary<DateOnly, ICollection<IForecastsFileRow>> _forecastedFileDictionary =
            new Dictionary<DateOnly, ICollection<IForecastsFileRow>>();

        public void Add(DateOnly date, IForecastsFileRow forecastsRow)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            if (_forecastedFileDictionary.TryGetValue(date, out forecastsRows))
            {
                if (forecastsRows.Contains(forecastsRow))
                    return;
                forecastsRows.Add(forecastsRow);
            }
            else
                _forecastedFileDictionary.Add(date, new List<IForecastsFileRow> { forecastsRow });
        }

        public ICollection<IForecastsFileRow> Get(DateOnly date)
        {
            ICollection<IForecastsFileRow> forecastsRows;
            _forecastedFileDictionary.TryGetValue(date, out forecastsRows);
            return forecastsRows;
        }
    }
}
