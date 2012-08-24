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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
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
				ISmsSender sender = null;
				if(_smsConfigReader.HasLoadedConfig)
				{
					//TODO Add error handling and logging of errors
					var type = Assembly.GetExecutingAssembly().GetType(_smsConfigReader.ClassName);
					sender =  (ISmsSender)Activator.CreateInstance(type);
				}
				// default
				if(sender == null)
					sender =  new ClickatellSmsSender();

				sender.SetConfigReader(_smsConfigReader);
				return sender;
			}
		}
	}
}