using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Authentication;
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
            	row["application_logon_name"] = person.ApplicationAuthenticationInfo == null
            	                                	? string.Empty
            	                                	: person.ApplicationAuthenticationInfo.ApplicationLogOnName;
            	row["password"] = person.ApplicationAuthenticationInfo == null
            	                  	? string.Empty
            	                  	: person.ApplicationAuthenticationInfo.Password;
			
				var logOn = person.AuthenticationInfo == null
				? new Tuple<string, string>(string.Empty, string.Empty)
				: IdentityHelper.Split(person.AuthenticationInfo.Identity);
			
            	row["windows_logon_name"] = logOn.Item2;
            	row["windows_domain_name"] = logOn.Item1;
                row["email"] = person.Email;
                row["language_id"] = person.PermissionInformation.UICultureLCID().GetValueOrDefault(-1);
                row["language_name"] = System.DBNull.Value;
                row["culture"] = person.PermissionInformation.CultureLCID().GetValueOrDefault(-1);
                row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(person);

                table.Rows.Add(row);
            }
        }
    }
}
