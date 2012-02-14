using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;

namespace Teleopti.Ccc.TestPayrollGui.Infrastructure
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
}
