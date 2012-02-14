using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class UserTransformer : IEtlTransformer<IPerson>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Transform(IEnumerable<IPerson> rootList, DataTable table)
        {
            InParameter.NotNull("rootList", rootList);
            InParameter.NotNull("table", table);

            foreach (IPerson person in rootList)
            {
                DataRow row = table.NewRow();

                row["person_code"] = person.Id;
                row["person_first_name"] = person.Name.FirstName;
                row["person_last_name"] = person.Name.LastName;
                row["application_logon_name"] =
                    person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName;
                row["windows_logon_name"] =
                    person.PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName;
                row["windows_domain_name"] =
                    person.PermissionInformation.WindowsAuthenticationInfo.DomainName;
                row["password"] = person.PermissionInformation.ApplicationAuthenticationInfo.Password;
                row["email"] = person.Email;
                row["language_id"] = person.PermissionInformation.UICultureLCID().GetValueOrDefault(-1);
                //row["language_name"] = CultureInfo.GetCultureInfo(person.PermissionInformation.UICultureLCID().GetValueOrDefault(1033)).ToString();
                row["language_name"] = System.DBNull.Value;
                row["culture"] = person.PermissionInformation.CultureLCID().GetValueOrDefault(-1);
                row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(person);

                table.Rows.Add(row);
            }
        }
    }
}
