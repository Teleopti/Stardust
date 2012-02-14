using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using Teleopti.Ccc.Sdk.Client;

namespace Teleopti.Ccc.Sdk.LoadTest
{
    public class ThreadLocalSoapHeaderExtension : SoapExtension
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
            var header = ThreadLocalAuthenticationSoapHeader.Current;
            header.MustUnderstand = false;
            message.Headers.Add(header);
        }
    }

    public class ThreadLocalAuthenticationSoapHeader : SoapHeader
    {
        [ThreadStatic]
        private static AuthenticationSoapHeader _authenticationSoapHeader;

        public static AuthenticationSoapHeader Current
        {
            get { return _authenticationSoapHeader ?? (_authenticationSoapHeader = new AuthenticationSoapHeader()); }
        }

        public static void Reset()
        {
            _authenticationSoapHeader = null;
        }
    }
}
