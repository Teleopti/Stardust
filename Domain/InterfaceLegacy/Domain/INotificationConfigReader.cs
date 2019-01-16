using System;
using System.Xml.XPath;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface INotificationConfigReader
	{
		bool HasLoadedConfig { get; }
		//IXPathNavigable XmlDocument { get; }
		Uri Url { get; }
		string User { get; }
		string Password { get; }
		string From { get; }
		string ClassName { get; }
		string Assembly { get; }
		string Api { get; }
		string Data { get; }
		string FindSuccessOrError { get; }
		string ErrorCode { get; }
		string SuccessCode { get; }
		bool SkipSearch { get; }
		string SmtpHost { get; }
		bool SmtpEnabled { get; }
		int SmtpPort { get; }
		bool SmtpUseSsl { get; }
		string SmtpUser { get; }
		string SmtpPassword { get; }
		bool SmtpUseRelay { get; }
		string ContentType { get; }
		string EncodingName { get; }
	}
}