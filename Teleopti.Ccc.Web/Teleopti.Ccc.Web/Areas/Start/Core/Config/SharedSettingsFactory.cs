using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Config
{
	public class SharedSettingsFactory : ISharedSettingsFactory
	{
		private readonly IConfigReader _configReader;

		public SharedSettingsFactory(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public SharedSettings Create()
		{
			return new SharedSettings
			{
				MessageBroker = _configReader.AppSettings["MessageBroker"],
				MessageBrokerLongPolling = _configReader.AppSettings["MessageBrokerLongPolling"],
				RtaPollingInterval = _configReader.AppSettings["RtaPollingInterval"],
				Queue = _configReader.ConnectionStrings["Queue"] == null ? 
					string.Empty : 
					Encryption.EncryptStringToBase64(_configReader.ConnectionStrings["Queue"].ToString(), EncryptionConstants.Image1, EncryptionConstants.Image2)
			};
		}
	}
}