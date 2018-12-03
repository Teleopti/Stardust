using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
    public class AgentSkillTransform : IEtlTransformer<IPerson>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Transform(IEnumerable<IPerson> rootList, DataTable table)
        {
            InParameter.NotNull("rootList", rootList);
            InParameter.NotNull("table", table);

            foreach (var person in rootList)
            {
                foreach (var personPeriod in person.PersonPeriodCollection)
                {
                    foreach (var personSkill in personPeriod.PersonSkillCollection)
                    {
                        AddRow(person, personPeriod, table, personSkill);
                    }
                }
            }
        }

        private static void AddRow(IPerson person, IPersonPeriod personPeriod, DataTable table, IPersonSkill personSkill)
        {
            DataRow row = table.NewRow();

            System.TimeZoneInfo TimeZoneInfo = person.PermissionInformation.DefaultTimeZone();

            row["skill_date"] = personPeriod.StartDate.Date;
            row["interval_id"] = -1;
            row["person_code"] = person.Id;
            if (personSkill.Skill.Id != null) row["skill_code"] = personSkill.Skill.Id;
            row["date_from"] = TimeZoneInfo.SafeConvertTimeToUtc(personPeriod.StartDate.Date);
            row["date_to"] = TimeZoneInfo.SafeConvertTimeToUtc(personPeriod.EndDate().Date);
            row["datasource_id"] = 1;
            row["active"] = personSkill.Active;

            table.Rows.Add(row);
        }
    }



}
