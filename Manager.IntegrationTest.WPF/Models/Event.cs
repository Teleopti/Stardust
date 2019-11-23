using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Manager.IntegrationTest.WPF.Models
{
	public class Event : INotifyPropertyChanged
	{
		private string _eventClass;
		private string _objectName;
		private string _sessionLoginName;
		private string _spid;
		private string _textData;

		public string EventClass
		{
			get { return _eventClass; }
			set
			{
				_eventClass = value;

				OnPropertyChanged();
			}
		}

		public string TextData
		{
			get { return _textData; }
			set
			{
				_textData = value;

				OnPropertyChanged();
			}
		}

		public string ObjectName
		{
			get { return _objectName; }
			set
			{
				_objectName = value;

				OnPropertyChanged();
			}
		}

		public string SessionLoginName
		{
			get { return _sessionLoginName; }
			set
			{
				_sessionLoginName = value;

				OnPropertyChanged();
			}
		}


		public string SPID
		{
			get { return _spid; }
			set
			{
				_spid = value;

				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

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