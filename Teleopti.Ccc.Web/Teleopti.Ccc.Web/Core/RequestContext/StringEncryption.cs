using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class StringEncryption
	{

	public string Encrypt(string stringToEncrypt)
    {
        FormsAuthenticationTicket tk = new FormsAuthenticationTicket(stringToEncrypt, false, 600);
        // returns encrypted string
        return FormsAuthentication.Encrypt(tk); 
    }


	public string Decrypt(string encryptedString)
    {
        FormsAuthenticationTicket tk= FormsAuthentication.Decrypt(encryptedString);
        return tk.Name;
    }
}