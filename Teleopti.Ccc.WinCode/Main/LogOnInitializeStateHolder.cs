using System;
using System.Configuration;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.WinCode.Services;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonInitializeStateHolder
	{
		public static void InitWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			LoadPasswordPolicyService passwordPolicyService;
			if (settings.PasswordPolicy == null)
			{
				//to be able start desktop app without shared setting server
				passwordPolicyService = new LoadPasswordPolicyService(Environment.CurrentDirectory);
			}
			else
			{
				var passwordPolicyDocument = XDocument.Parse(settings.PasswordPolicy);
				passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);
			}

			var appSettings = settings.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary());

			var initializer = new InitializeApplication(messageBroker);

			initializer.Start(new StateManager(), passwordPolicyService, appSettings, true);
		}
	}
}