﻿using System;
using System.IO;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Client
{
    public class AuthenticationSoapHeaderExtension : SoapExtension
    {
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }

        public override object GetInitializer(Type webServiceType)
        {
            return null;
        }

        public override void Initialize(object initializer)
        {
            return;
        }

        public override Stream ChainStream(Stream stream)
        {
            return stream;
        }

        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    //Add the CustomSoapHeader to outgoing client requests
                    if (message is SoapClientMessage)
                    {
                        AddHeader(message);
                    }
                    break;

                case SoapMessageStage.AfterSerialize:
                    break;

                case SoapMessageStage.BeforeDeserialize:
                    break;

                case SoapMessageStage.AfterDeserialize:
                    break;
            }
        }

        private static void AddHeader(SoapMessage message)
        {
            var header = AuthenticationSoapHeader.Current;
            header.MustUnderstand = false;
            message.Headers.Add(header);
        }
    }

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
}
