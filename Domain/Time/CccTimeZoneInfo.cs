using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    [Serializable]
    public sealed class CccTimeZoneInfo : ICccTimeZoneInfo
    {
        private readonly TimeZoneInfo _timeZoneInfo;
        
        public CccTimeZoneInfo()
        {
        }

        public CccTimeZoneInfo(TimeZoneInfo timeZoneInfo) :this()
        {
            _timeZoneInfo = timeZoneInfo;
        }

        // Not used anywhere, Henry 2008-11-03
        // But fx cop will cry ;-) Tamas 2008-11-03
        private CccTimeZoneInfo(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById((string) info.GetValue("Id", typeof (string)));
        }


        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Id", _timeZoneInfo.Id);
        }

        public bool Equals(ICccTimeZoneInfo other)
        {
            return _timeZoneInfo.Equals((TimeZoneInfo)other.TimeZoneInfoObject);
        }


        public TimeSpan GetUtcOffset(DateTime dateTime)
        {
            return _timeZoneInfo.GetUtcOffset(dateTime);
        }

        public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset)
        {
            return _timeZoneInfo.GetUtcOffset(dateTimeOffset);
        }

        public bool IsAmbiguousTime(DateTime dateTime)
        {
            return _timeZoneInfo.IsAmbiguousTime(dateTime);
        }

        public bool IsAmbiguousTime(DateTimeOffset dateTimeOffset)
        {
            return _timeZoneInfo.IsAmbiguousTime(dateTimeOffset);
        }

        public bool IsDaylightSavingTime(DateTime dateTime)
        {
            return _timeZoneInfo.IsDaylightSavingTime(dateTime);
        }

        public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
        {
            return _timeZoneInfo.IsDaylightSavingTime(dateTimeOffset);
        }

        public bool IsInvalidTime(DateTime dateTime)
        {
            return _timeZoneInfo.IsInvalidTime(dateTime);
        }

        public string ToSerializedString()
        {
            return _timeZoneInfo.ToSerializedString();
        }

        public TimeSpan BaseUtcOffset
        {
            get { return _timeZoneInfo.BaseUtcOffset; }
        }

        public string DaylightName
        {
            get { return _timeZoneInfo.DaylightName; }
        }

        public string DisplayName
        {
            get { return _timeZoneInfo.DisplayName; }
        }

        public string Id
        {
            get { return _timeZoneInfo.Id; }
        }

        public string StandardName
        {
            get { return _timeZoneInfo.StandardName; }
        }

        public bool SupportsDaylightSavingTime
        {
            get { return _timeZoneInfo.SupportsDaylightSavingTime; }
        }

        public object TimeZoneInfoObject
        {
            get { return _timeZoneInfo; }
        }

        public ICccTimeZoneInfo Utc
        {
            get{ return new CccTimeZoneInfo(TimeZoneInfo.Utc); }
        }

        public DateTime ConvertTimeFromUtc(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, (TimeZoneInfo)TimeZoneInfoObject);
        }

        public DateTime ConvertTimeFromUtc(DateTime startDateTime, ICccTimeZoneInfo timeZoneInfo)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(startDateTime, (TimeZoneInfo)timeZoneInfo.TimeZoneInfoObject);
        }

        public DateTime ConvertTimeToUtc(DateTime dateTime)
        {
            return ConvertTimeToUtc(dateTime,this);
        }

        public DateTime ConvertTimeToUtc(DateTime dateTime, ICccTimeZoneInfo timeZoneInfo)
        {
            var kindUnSpecifiedDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            while (timeZoneInfo.IsInvalidTime(kindUnSpecifiedDateTime))
            {
                kindUnSpecifiedDateTime = kindUnSpecifiedDateTime.AddMinutes(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(kindUnSpecifiedDateTime, (TimeZoneInfo)timeZoneInfo.TimeZoneInfoObject);
        }

    }
}
