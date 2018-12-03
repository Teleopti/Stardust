using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
	public abstract class AbstractPropertyPagesNoRoot<T> : IAbstractPropertyPagesNoRoot<T>, IDisposable
	{
		private IPropertyPageNoRoot<T> _currentPage;
		private List<IPropertyPageNoRoot<T>> _propertyPages;
		private Form _owningForm;
		private T _stateObj;

		public Form Owner
		{
			get { return _owningForm; }
			set { _owningForm = value; }
		}

		public void ChangePages(IEnumerable<IPropertyPageNoRoot<T>> newPages)
		{
			var indexOfPage = _propertyPages.IndexOf(_currentPage);
			_propertyPages.Clear();
			_propertyPages.AddRange(newPages);
			_currentPage = _propertyPages[indexOfPage];

			var pageListChanged = PageListChanged;
			if (pageListChanged != null)
			{
				pageListChanged(this, EventArgs.Empty);
			}
		}

		public virtual Size MinimumSize
		{
			get
			{
				return new Size(550, 400);
			}
		}

		public abstract T CreateNewStateObj();

		public abstract string Name { get; }

		public void Save()
		{
			CurrentPage.Depopulate(_stateObj);
		}

		public event EventHandler PageListChanged;

		public abstract string WindowText { get; }

		protected AbstractPropertyPagesNoRoot(T stateObj)
		{
			_stateObj = stateObj;
		}

		public void Initialize(IList<IPropertyPageNoRoot<T>> pages)
		{
			InParameter.NotNull("pages", pages);
			_propertyPages = new List<IPropertyPageNoRoot<T>>(pages);
		}

		public ReadOnlyCollection<IPropertyPageNoRoot<T>> Pages
		{
			get
			{
				return new ReadOnlyCollection<IPropertyPageNoRoot<T>>(_propertyPages);
			}
		}

		public IPropertyPageNoRoot<T> FirstPage
		{
			get { return _propertyPages[0]; }
		}

		public IPropertyPageNoRoot<T> CurrentPage
		{
			get
			{
				if (_currentPage == null)
				{
					_currentPage = FirstPage;
				}
				return _currentPage;
			}
			set { _currentPage = value; }
		}

		public IPropertyPageNoRoot<T> PreviousPage()
		{
			CurrentPage = FindMyPreviousPage(CurrentPage);
			CurrentPage.Populate(StateObj);

			return CurrentPage;
		}

		public IPropertyPageNoRoot<T> NextPage()
		{
			if (CurrentPage.Depopulate(StateObj))
			{
				CurrentPage = FindMyNextPage(CurrentPage);
				CurrentPage.Populate(StateObj);
			}
			return CurrentPage;
		}

		public bool IsOnFirst()
		{
			return (CurrentPage == _propertyPages[0]);
		}

		public bool IsOnLast()
		{
			return (CurrentPage == _propertyPages[_propertyPages.Count - 1]);
		}

		public String[] GetPageNames()
		{
			return _propertyPages
				.Select(pp => pp.PageName)
				.ToArray();
		}

		public T StateObj
		{
			get
			{
				if (_stateObj == null)
				{
					_stateObj = CreateNewStateObj();
				}
				return _stateObj;
			}
			protected set { _stateObj = value; }
		}

		public IPropertyPageNoRoot<T> FindMyNextPage(IPropertyPageNoRoot<T> propertyPage)
		{
			return findPageByOffset(propertyPage, 1);
		}

		public IPropertyPageNoRoot<T> FindMyPreviousPage(IPropertyPageNoRoot<T> propertyPage)
		{
			return findPageByOffset(propertyPage, -1);
		}

		private IPropertyPageNoRoot<T> findPageByOffset(IPropertyPageNoRoot<T> propertyPage, int offset)
		{
			int myIndex = _propertyPages.IndexOf(propertyPage);
			var page = _propertyPages.ElementAtOrDefault(myIndex + offset);
			if (page == null) page = propertyPage;

			return page;
		}

		public IPropertyPageNoRoot<T> ShowPage(IPropertyPageNoRoot<T> propertyPage)
		{
			bool okToSwitchPage = true;
			if (_currentPage != null)
			{
				okToSwitchPage = CurrentPage.Depopulate(StateObj);
			}
			if (okToSwitchPage)
			{
				CurrentPage = propertyPage;
				CurrentPage.Populate(StateObj);
			}
			return CurrentPage;
		}

		public virtual bool ModeCreateNew
		{
			get { return false; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				if (_owningForm != null)
				{
					_owningForm.Dispose();
				}

				//Free all intialized pages
				if (_propertyPages != null)
				{
					foreach (var propertyPage in _propertyPages)
					{
						propertyPage.Dispose();
					}
				}
			}
		}

		#endregion
	}
}
