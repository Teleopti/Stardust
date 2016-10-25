using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class PagingDetail : INotifyPropertyChanged
	{
		private Paging _paging = new Paging{Take = 20};

		public int TotalNumberOfResults
		{
			get { return _paging.TotalCount; }
			set
			{
				_paging = new Paging {TotalCount = value, Skip = _paging.Skip, Take = _paging.Take};
				notifyPropertyChanged("TotalNumberOfResults");
			}
		}

		public int Take
		{
			get { return _paging.Take; }
			set
			{
				_paging = new Paging { TotalCount = _paging.TotalCount, Skip = _paging.Skip, Take = value };
				notifyPropertyChanged("Take");
			}
		}

		public int Skip
		{
			get { return _paging.Skip; }
			set
			{
				_paging = new Paging { TotalCount = _paging.TotalCount, Skip = value, Take = _paging.Take };
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