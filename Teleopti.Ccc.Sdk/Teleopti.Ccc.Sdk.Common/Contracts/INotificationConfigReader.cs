using System;
using System.Xml.XPath;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	public interface INotificationConfigReader
	{
		bool HasLoadedConfig { get; }
		IXPathNavigable XmlDocument { get; }
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
	    INotificationClient CreateClient();
	}
}