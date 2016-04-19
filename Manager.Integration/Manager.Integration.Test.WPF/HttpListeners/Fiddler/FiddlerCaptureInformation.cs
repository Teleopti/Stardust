using System.ComponentModel;
using System.Runtime.CompilerServices;
using Manager.Integration.Test.WPF.Annotations;

namespace Manager.Integration.Test.WPF.HttpListeners.Fiddler
{
	public class FiddlerCaptureInformation : INotifyPropertyChanged
	{
		private string _uri;

		public string Uri
		{
			get { return _uri; }
			set
			{
				_uri = value;

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