using System;
using System.ComponentModel;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class PagingDetail : INotifyPropertyChanged
	{
		private readonly Paging _paging = new Paging{Take = 20};

		public int TotalNumberOfResults
		{
			get { return _paging.TotalCount; }
			set
			{
				_paging.TotalCount = value;
				notifyPropertyChanged("TotalNumberOfResults");
			}
		}

		public int Take
		{
			get { return _paging.Take; }
			set
			{
				_paging.Take = value;
				notifyPropertyChanged("Take");
			}
		}

		public int Skip
		{
			get { return _paging.Skip; }
			set
			{
				_paging.Skip = value;
				notifyPropertyChanged("Skip");
			}
		}

		private void notifyPropertyChanged(string property)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler.Invoke(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}