using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public class PersonAccountModel
    {
        private readonly PersonAccountDto _personAccountDto;

        public PersonAccountModel(PersonAccountDto personAccountDto)
        {
            _personAccountDto = personAccountDto;
            Description = _personAccountDto.TrackingDescription;
            PeriodFrom = _personAccountDto.Period.StartDate;
            PeriodTo = _personAccountDto.Period.EndDate;

            if (PeriodTo.DateTime == PeriodFrom.DateTime.AddDays(3600)) //Default period end set when having no next period
                EndDate = null;
            else
                EndDate = _personAccountDto.Period.EndDate;

            if (_personAccountDto.IsInMinutes)
            {
                TypeOfValue = UserTexts.Resources.Hours;
                var accruedMinutes = _personAccountDto.Accrued + _personAccountDto.BalanceIn + _personAccountDto.Extra;
                Accrued = TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(accruedMinutes),CultureInfo.CurrentCulture);
                Used = TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(_personAccountDto.LatestCalculatedBalance), CultureInfo.CurrentCulture);
                Remaining = TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(_personAccountDto.Remaining), CultureInfo.CurrentCulture);
            }
            else
            {
                TypeOfValue = UserTexts.Resources.Days;
                var accruedDays = _personAccountDto.Accrued + _personAccountDto.BalanceIn + _personAccountDto.Extra;
                Accrued = new TimeSpan(accruedDays).Days.ToString(CultureInfo.CurrentCulture);
                Used = new TimeSpan(_personAccountDto.LatestCalculatedBalance).Days.ToString(CultureInfo.CurrentCulture);
                Remaining = new TimeSpan(_personAccountDto.Remaining).Days.ToString(CultureInfo.CurrentCulture);
            }
        }

        public string Description { get; private set; }

        public DateOnlyDto PeriodFrom { get; private set; }

        public DateOnlyDto PeriodTo { get; private set; }

        public DateOnlyDto EndDate { get; private set; }

        public string TypeOfValue { get; private set; }

        public string Accrued { get; private set; }

        public string Used { get; private set; }

        public string Remaining { get; private set; }
    }
}