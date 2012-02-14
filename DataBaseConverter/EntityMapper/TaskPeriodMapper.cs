using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Task Period mapper
    /// </summary>
    public class TaskPeriodMapper : Mapper<TemplateTaskPeriod, global::Domain.ForecastData>
    {
        private readonly int _intervalLength;
        private readonly TimeSpan _intervalLengthAsTimeSpan;
        private readonly DateTime _dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPeriodMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public TaskPeriodMapper(MappedObjectPair mappedObjectPair,
                                ICccTimeZoneInfo timeZone,
                                DateTime dateTime,
                                int intervalLength)
            : base(mappedObjectPair,timeZone)
        {
            _intervalLength = intervalLength;
            _dateTime = dateTime;
            _intervalLengthAsTimeSpan = TimeSpan.FromMinutes(_intervalLength);
        }


        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public override TemplateTaskPeriod Map(global::Domain.ForecastData oldEntity)
        {
            TaskMapper taskMapper = new TaskMapper();
            Task newTask = taskMapper.Map(oldEntity);

            TimeSpan ts = TimeSpan.FromMinutes(_intervalLength * oldEntity.Interval);
            TimeSpan endTs = ts.Add(_intervalLengthAsTimeSpan);

            DateTimePeriod tp = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                _dateTime.Add(ts), 
                _dateTime.Add(endTs), 
                TimeZone);
            //DateTimePeriod tp = new DateTimePeriod(_dateTime.Add(ts), _dateTime.Add(endTs));

            return new TemplateTaskPeriod(newTask, tp);
        }

        
    }
}