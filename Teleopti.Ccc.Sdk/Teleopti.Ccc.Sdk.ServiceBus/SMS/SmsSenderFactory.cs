using System;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public interface ISmsSenderFactory
	{
		ISmsSender Sender { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsSenderFactory : ISmsSenderFactory
	{
		private readonly ISmsConfigReader _smsConfigReader;

		public SmsSenderFactory(ISmsConfigReader smsConfigReader)
		{
			_smsConfigReader = smsConfigReader;
		}

		//
		//create via reflection of the class from config file
		public ISmsSender Sender
		{
			get
			{
				if(_smsConfigReader.HasLoadedConfig)
				{
					//TODO Add error handling and logging of errors
					var type = Assembly.GetExecutingAssembly().GetType(_smsConfigReader.Class);
					return (ISmsSender)Activator.CreateInstance(type);
				}
				// default
				return new ClickatellSmsSender(_smsConfigReader);
			}
		}
	}
}