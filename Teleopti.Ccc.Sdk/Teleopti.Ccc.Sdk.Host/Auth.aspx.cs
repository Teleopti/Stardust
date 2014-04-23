using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.WcfService;

namespace Teleopti.Ccc.Sdk.WcfHost
{
	public partial class WebForm1 : System.Web.UI.Page
	{
		private string _phrase = string.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected void Page_Load(object sender, EventArgs e)
		{
#if !DEBUG
			throw new ApplicationException("Only available in debug mode.");
#else
			var passphrase = new PassphraseFromConfiguration();
			_phrase = passphrase.Passphrase();
			if (string.IsNullOrEmpty(_phrase))
				throw new ApplicationException("Only available in debug mode when having a passphrase set.");

			if (!Page.IsPostBack)
			{
				url.Text = "http://localhost/clickonce/Teleopti.Ccc.AgentPortal.application";
				dataSources.DataSource =
					StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.Select(d => d.Application.Name)
						.
						ToArray();
				dataSources.DataBind();
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
		protected void Unnamed1_Click(object sender, EventArgs e)
		{
			var settings = (NameValueCollection) ConfigurationManager.GetSection("teleopti/publishedSettings");
			var key = Convert.FromBase64String(settings["SharedKey"]);
			var iv = Convert.FromBase64String(settings["SharedIV"]);

			var givenUserName = userName.Text;
			var oneWayEncryption = new OneWayEncryption();
			var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

			var encryption =
				Encryption.EncryptStringToBase64(
					string.Format("usr={0},pwd={1},datasource={2},time={3}", givenUserName,
					              oneWayEncryption.EncryptStringWithBase64(_phrase, givenUserName), dataSources.Text, time), key, iv);

			Response.Clear();
			if (RadioButtonList1.SelectedValue=="ClickOnce")
			{
				Response.Redirect(url.Text + "?" + encryption,true);
			}
			else
			{
				Response.Write(@"<script type=""text/javascript"">window.location='mytime://?" + encryption + "';history.go(-1);</script><a href='javascript:history.go(-1);'>Back</a>");
				Response.End();
			}
		}
	}
}