using System.ComponentModel;
using System.Runtime.CompilerServices;
using Manager.IntegrationTest.WPF.Annotations;

namespace Manager.IntegrationTest.WPF.HttpListeners.Fiddler
{
	public class FiddlerCaptureInformation : INotifyPropertyChanged
	{
		private string _uri;
		private int _responseCode;
		private string _requestHeaders;
		private string _requestBody;
		private string _requestMethod;

		public string RequestMethod
		{
			get { return _requestMethod; }
			set
			{
				_requestMethod = value;

				OnPropertyChanged();
			}
		}

		public string RequestBody
		{
			get { return _requestBody; }
			set
			{
				_requestBody = value;

				OnPropertyChanged();
			}
		}

		public string RequestHeaders
		{
			get { return _requestHeaders; }
			set
			{
				_requestHeaders = value;

				OnPropertyChanged();
			}
		}

		public string Uri
		{
			get { return _uri; }
			set
			{
				_uri = value;

				OnPropertyChanged();
			}
		}

		public int ResponseCode
		{
			get { return _responseCode; }
			set
			{
				_responseCode = value;

				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}