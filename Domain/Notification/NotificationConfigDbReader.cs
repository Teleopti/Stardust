using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationConfigDbReader : INotificationConfigReader
	{

		// TODO FIX
		public bool HasLoadedConfig => throw new NotImplementedException();

		public IXPathNavigable XmlDocument => throw new NotImplementedException();

		public Uri Url => throw new NotImplementedException();

		public string User => throw new NotImplementedException();

		public string Password => throw new NotImplementedException();

		public string From => throw new NotImplementedException();

		public string ClassName => throw new NotImplementedException();

		public string Assembly => throw new NotImplementedException();

		public string Api => throw new NotImplementedException();

		public string Data => throw new NotImplementedException();

		public string FindSuccessOrError => throw new NotImplementedException();

		public string ErrorCode => throw new NotImplementedException();

		public string SuccessCode => throw new NotImplementedException();

		public bool SkipSearch => throw new NotImplementedException();

		public string SmtpHost => throw new NotImplementedException();

		public int SmtpPort => throw new NotImplementedException();

		public bool SmtpUseSsl => throw new NotImplementedException();

		public string SmtpUser => throw new NotImplementedException();

		public string SmtpPassword => throw new NotImplementedException();

		public bool SmtpUseRelay => throw new NotImplementedException();

		public string ContentType => throw new NotImplementedException();

		public string EncodingName => throw new NotImplementedException();
	}
}