using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Text;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class SingleSignOnHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static string SingleSignOn()
        {
            var token = Guid.NewGuid();
            using (var customerWebProxy = new CustomerWebProxy())
            {
                var ticket = new Hashtable();
                IPerson currentPerson;
                using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    currentPerson = TeleoptiPrincipal.Current.GetPerson(new PersonRepository(unitOfWork));
                }
                var email = currentPerson.Email;
                if (string.IsNullOrEmpty(email))
                    throw new ArgumentException(
                        UserTexts.Resources.YourEmailAddressShouldBeConfiguredInThePeopleModuleToUseThisServiceDot);
                ticket.Add("email", email);
                ticket.Add("firstname", currentPerson.Name.FirstName);
                ticket.Add("lastname", currentPerson.Name.LastName);
                ticket.Add("guid", token);
                var iv = "qwertyui";
                var key = "12345678";
                var encryptedData = SerializeData(ticket, Encoding.UTF8.GetBytes(iv), Encoding.UTF8.GetBytes(key));
                var result = customerWebProxy.InitializeSSOArray(new InitializeSSOArrayRequest(encryptedData));

                if (result.InitializeSSOArrayResult.Status != 1)
                {
                    throw new ArgumentException(result.InitializeSSOArrayResult.Message);
                }
            }
            return "guid=" + token;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static byte[] SerializeData(Hashtable hashtable, byte[] iv, byte[] key)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.IV = iv;
                des.Key = key;

                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    SoapFormatter formatter = new SoapFormatter();
                    formatter.Serialize(cs, hashtable);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }
    }
}
