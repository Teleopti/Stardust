using System;
using System.Linq;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.WcfExtensions
{
	public class TeleoptiAuthenticationHeader : MessageHeader
	{
		public TeleoptiAuthenticationHeader(string userName, string password, string dataSource, Guid businessUnit)
		{
			UseWindowsIdentity = false;
			UserName = userName;
			Password = password;
			DataSource = dataSource;
			BusinessUnit = businessUnit;
		}

		public Guid BusinessUnit { get; }

		public string DataSource { get; }

		public string Password { get; }

		public string UserName { get; }

		public bool UseWindowsIdentity { get; set; }

		public override string Name => TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderName;

		public override string Namespace => TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace;

		protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
		{
			writer.WriteElementString(TeleoptiAuthenticationHeaderNames.UserNameKey, UserName);
			writer.WriteElementString(TeleoptiAuthenticationHeaderNames.PasswordKey, Password);
			writer.WriteElementString(TeleoptiAuthenticationHeaderNames.DataSourceKey, DataSource);
			writer.WriteElementString(TeleoptiAuthenticationHeaderNames.BusinessUnitKey, XmlConvert.ToString(BusinessUnit));
			writer.WriteElementString(TeleoptiAuthenticationHeaderNames.UseWindowsIdentityKey,
				XmlConvert.ToString(UseWindowsIdentity));
		}

		public static TeleoptiAuthenticationHeader ReadHeader(XmlReader reader)
		{
			// Read the header content (key) using the XmlDictionaryReader

			var document = XDocument.Parse(reader.ReadOuterXml());

			var element = document.Descendants(
				XName.Get(TeleoptiAuthenticationHeaderNames.UserNameKey,
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)).FirstOrDefault();

			string username = string.Empty;
			if (element != null)
				username = element.Value;

			element = document.Descendants(
				XName.Get(TeleoptiAuthenticationHeaderNames.PasswordKey,
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)).FirstOrDefault();

			string password = string.Empty;
			if (element != null)
				password = element.Value;

			element = document.Descendants(
				XName.Get(TeleoptiAuthenticationHeaderNames.DataSourceKey,
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)).FirstOrDefault();

			string datasource = string.Empty;
			if (element != null)
				datasource = element.Value;

			element = document.Descendants(
				XName.Get(TeleoptiAuthenticationHeaderNames.BusinessUnitKey,
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)).FirstOrDefault();

			Guid businessUnit = Guid.Empty;
			if (element != null)
			{
				Guid.TryParse(element.Value, out businessUnit);
			}

			element = document.Descendants(
				XName.Get(TeleoptiAuthenticationHeaderNames.UseWindowsIdentityKey,
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace)).FirstOrDefault();

			bool useWindowsIdentity = false;
			if (element != null)
			{
				try
				{
					useWindowsIdentity = XmlConvert.ToBoolean(element.Value);
				}
				catch (Exception)
				{
				}
            }

            return new TeleoptiAuthenticationHeader(username, password, datasource, businessUnit)
                       {UseWindowsIdentity = useWindowsIdentity};
        }
    }
}