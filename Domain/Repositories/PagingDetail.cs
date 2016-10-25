using System.ComponentModel;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class PagingDetail : INotifyPropertyChanged
	{
		private int _skip;
		private int _take = 20;
		private int _totalNumberOfResults;

		public int TotalNumberOfResults
		{
			get { return _totalNumberOfResults; }
			set
			{
				_totalNumberOfResults = value;
				notifyPropertyChanged(nameof(TotalNumberOfResults));
			}
		}

		public int Take
		{
			get { return _take; }
			set
			{
				_take = value;
				notifyPropertyChanged(nameof(Take));
			}
		}

		public int Skip
		{
			get { return _skip; }
			set
			{
				_skip = value;
				notifyPropertyChanged(nameof(Skip));
			}
		}

		private void notifyPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}