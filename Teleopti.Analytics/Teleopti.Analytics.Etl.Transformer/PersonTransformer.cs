using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Transformer
{
    public static class PersonTransformer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void Transform(IEnumerable<IPerson> personCollection, int intervalsPerDay, DateOnly insertDate, DataTable personTable, DataTable acdLogOnTable, ICommonNameDescriptionSetting commonNameDescriptionSetting)
        {
            InParameter.NotNull("personCollection", personCollection);

            foreach (IPerson person in personCollection)
            {
                TimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

                foreach (IPersonPeriod personPeriod in person.PersonPeriodCollection)
                {
                    createPersonDataRow(person, personTable, timeZoneInfo, personPeriod, intervalsPerDay, insertDate, commonNameDescriptionSetting);
                    externalLogOnPerson(person, acdLogOnTable, timeZoneInfo, personPeriod);
                }
            }
        }

        private static void createPersonDataRow(IPerson person, DataTable table, TimeZoneInfo timeZoneInfo, IPersonPeriod personPeriod, int intervalsPerDay, DateOnly insertDate, ICommonNameDescriptionSetting commonNameDescriptionSetting)
        {
            DataRow row = table.NewRow();

            DateTime validFromDate = timeZoneInfo.SafeConvertTimeToUtc(personPeriod.StartDate.Date);
            DateTime validToDate = getPeriodEndDate(personPeriod.EndDate().Date, timeZoneInfo);
            row["person_code"] = person.Id;
            row["valid_from_date"] = validFromDate;
            row["valid_to_date"] = validToDate;
            row["valid_from_interval_id"] = new IntervalBase(validFromDate, intervalsPerDay).Id;
            row["valid_to_interval_id"] =
                new IntervalBase(GetPeriodIntervalEndDate(validToDate, intervalsPerDay), intervalsPerDay).Id;
            row["valid_to_interval_start"] = GetPeriodIntervalEndDate(validToDate, intervalsPerDay);

            if (personPeriod.Id.HasValue)
            {
                row["person_period_code"] = personPeriod.Id;
            }

            row["person_name"] = commonNameDescriptionSetting.BuildCommonNameDescription(person);
            row["person_first_name"] = person.Name.FirstName;
            row["person_last_name"] = person.Name.LastName;
            row["team_code"] = personPeriod.Team.Id;
            row["team_name"] = personPeriod.Team.Description.Name;

            if (personPeriod.Team.Scorecard != null && 
                !((IDeleteTag)personPeriod.Team).IsDeleted && 
                !((IDeleteTag)personPeriod.Team.Site).IsDeleted)
            {
                row["scorecard_code"] = personPeriod.Team.Scorecard.Id;
            }

            row["site_code"] = personPeriod.Team.Site.Id;
            row["site_name"] = personPeriod.Team.Site.Description.Name;
            row["business_unit_code"] = personPeriod.Team.BusinessUnitExplicit.Id;
            row["business_unit_name"] = personPeriod.Team.BusinessUnitExplicit.Name;
            row["employment_number"] = person.EmploymentNumber;
            row["employment_start_date"] = timeZoneInfo.SafeConvertTimeToUtc(personPeriod.StartDate.Date);
            row["employment_end_date"] = validToDate;
            row["is_agent"] = person.IsAgent(insertDate);
            row["is_user"] = false; //Actually "Not Defined"
            row["email"] = person.Email;
            row["note"] = person.Note;
            row["time_zone_id"] = person.PermissionInformation.DefaultTimeZone().Id;
            if (personPeriod.PersonContract.Contract.Id.HasValue)
                row["contract_code"] = personPeriod.PersonContract.Contract.Id;
            row["contract_name"] = personPeriod.PersonContract.Contract.Description;
            if (personPeriod.PersonContract.PartTimePercentage.Id.HasValue)
                row["parttime_code"] = personPeriod.PersonContract.PartTimePercentage.Id;
            row["parttime_percentage"] = personPeriod.PersonContract.PartTimePercentage.Percentage.Value * 100 + "%";
            row["employment_type"] = personPeriod.PersonContract.Contract.EmploymentType;
            row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
            row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(person);

	        var logOn = person.AuthenticationInfo == null
		        ? new Tuple<string, string>(string.Empty, string.Empty)
		        : IdentityHelper.Split(person.AuthenticationInfo.Identity);
			row["windows_domain"] = logOn.Item1;
	        row["windows_username"] = logOn.Item2;
            table.Rows.Add(row);
        }

        private static void externalLogOnPerson(IPerson person, DataTable acdLoginTable, TimeZoneInfo timeZoneInfo, IPersonPeriod personPeriod)
        {
            DateTime validFromDate = timeZoneInfo.SafeConvertTimeToUtc(personPeriod.StartDate.Date);
            DateTime validToDate = getPeriodEndDate(personPeriod.EndDate().Date, timeZoneInfo);

            foreach (IExternalLogOn externalLogOn in personPeriod.ExternalLogOnCollection)
            {
                DataRow row = acdLoginTable.NewRow();
                row["acd_login_code"] = externalLogOn.AcdLogOnOriginalId;
                row["person_code"] = person.Id;
                row["start_date"] = validFromDate;
                row["end_date"] = validToDate;
                if (personPeriod.Id.HasValue)
                {
                    row["person_period_code"] = personPeriod.Id;
                } 
                row["log_object_datasource_id"] = externalLogOn.DataSourceId;
                
                row["datasource_id"] = 1;
                acdLoginTable.Rows.Add(row);
            }
        }

        private static DateTime getPeriodEndDate(DateTime endDate, TimeZoneInfo timeZoneInfo)
        {
            if (endDate.Equals(new DateTime(2059, 12, 31)))
            {
                return endDate;
            }

            return timeZoneInfo.SafeConvertTimeToUtc(endDate.AddDays(1));
        }

        public static DateTime GetPeriodIntervalEndDate(DateTime endDate, int intervalsPerDay)
        {
            if (endDate.Equals(new DateTime(2059, 12, 31)))
            {
                return endDate;
            }

            int minutesPerInterval = 1440/intervalsPerDay;
            return endDate.AddMinutes(-minutesPerInterval);
        }
    }
}
