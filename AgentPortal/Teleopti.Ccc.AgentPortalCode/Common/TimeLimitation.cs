using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public interface ITimeLimitationValidator
    {
        bool Validate(TimeSpan? minTime, TimeSpan? maxTime);
        string Format(TimeSpan? value);
    }

    public class TimeOfDayValidator : ITimeLimitationValidator
    {
        private readonly bool _allowNextDay;

        public TimeOfDayValidator(bool allowNextDay)
        {
            _allowNextDay = allowNextDay;
        }

        private static readonly TimeSpan DayBreak = TimeSpan.FromHours(24);

        public bool Validate(TimeSpan? minTime, TimeSpan? maxTime)
        {
            bool nextDayIsValid = true;
            if (!_allowNextDay)
            {
                nextDayIsValid = minTime < DayBreak && maxTime < DayBreak;
            }
            return (!minTime.HasValue || !maxTime.HasValue) ||
                   (minTime.Value <= maxTime.Value && nextDayIsValid);
        }

        public string Format(TimeSpan? value)
        {
            string ret = "";
            if (value.HasValue)
            {
				ret = TimeHelper.TimeOfDayFromTimeSpan(value.Value, CultureInfo.CurrentCulture);
            }
            return ret;
        }
    }

    public class TimeLengthValidator : ITimeLimitationValidator
    {
        public bool Validate(TimeSpan? minTime, TimeSpan? maxTime)
        {
            return (!minTime.HasValue || !maxTime.HasValue) ||
                   (minTime.Value <= maxTime.Value);
        }

        public string Format(TimeSpan? value)
        {
            string ret = "";
            if (value.HasValue)
            {
                ret = TimeHelper.GetLongHourMinuteTimeString(value.Value, CultureInfo.CurrentCulture);
            }
            return ret;
        }
    }

    public class TimeLimitation : ICloneable
    {
        private readonly ITimeLimitationValidator _timeLimitationValidator;

        public TimeLimitation(ITimeLimitationValidator timeLimitationValidator)
        {
            _timeLimitationValidator = timeLimitationValidator;
        }

        public TimeLimitation(ITimeLimitationValidator timeLimitationValidator, TimeLimitationDto timeLimitationDto):this(timeLimitationValidator)
        {
            if (!string.IsNullOrEmpty(timeLimitationDto.MinTime))
                MinTime = System.Xml.XmlConvert.ToTimeSpan(timeLimitationDto.MinTime);
            if (!string.IsNullOrEmpty(timeLimitationDto.MaxTime))
                MaxTime = System.Xml.XmlConvert.ToTimeSpan(timeLimitationDto.MaxTime);
        }

        public void SetValuesToDto(TimeLimitationDto timeLimitationDto)
        {
            if (MinTime.HasValue)
                timeLimitationDto.MinTime = System.Xml.XmlConvert.ToString(MinTime.Value);
            if (MaxTime.HasValue)
                timeLimitationDto.MaxTime = System.Xml.XmlConvert.ToString(MaxTime.Value);
        }

        public TimeSpan? MinTime { get; set; }
        public TimeSpan? MaxTime { get; set; }

        public bool IsValid()
        {
            return _timeLimitationValidator.Validate(MinTime, MaxTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Exposed for handling Validation in the presentation-layer
        /// Created by: henrika
        /// Created date: 2009-01-27
        /// </remarks>
        private string StringFromTimeSpan(TimeSpan? value)
        {
            return _timeLimitationValidator.Format(value);
        }

        /// <summary>
        /// Gets or sets the start time string.
        /// </summary>
        /// <value>The start time string.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// </remarks>
        public string StartTimeString
        {
            get
            {
                return StringFromTimeSpan(MinTime);
            }
        }

        /// <summary>
        /// Gets or sets the end time string.
        /// </summary>
        /// <value>The end time string.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// </remarks>
        public string EndTimeString
        {
            get
            {
                return StringFromTimeSpan(MaxTime);
            }
        }

        public bool HasValue
        {
            get { return MaxTime.HasValue || MinTime.HasValue; }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return MinTime.GetHashCode() ^ MaxTime.GetHashCode();
        }
    }
}
