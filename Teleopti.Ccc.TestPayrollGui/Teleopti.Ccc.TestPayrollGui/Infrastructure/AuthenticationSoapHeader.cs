using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Teleopti.Ccc.TestPayrollGui.Infrastructure
{
    [XmlRoot(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderName, Namespace = TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)]
    public class AuthenticationSoapHeader : SoapHeader
    {
        private static AuthenticationSoapHeader _authenticationSoapHeader = new AuthenticationSoapHeader();

        public string UserName { get; set; }
        public string Password { get; set; }
        public string DataSource { get; set; }
        public string BusinessUnit { get; set; }
        public bool UseWindowsIdentity { get; set; }

        public static AuthenticationSoapHeader Current
        {
            get { return _authenticationSoapHeader; }
        }
    }

    public static class TeleoptiAuthenticationHeaderNames
    {
        public const string TeleoptiAuthenticationHeaderName = "TeleoptiAuthenticationHeader";

        public const string TeleoptiAuthenticationHeaderNamespace = "http://schemas.ccc.teleopti.com/2011/02";
    }
}
