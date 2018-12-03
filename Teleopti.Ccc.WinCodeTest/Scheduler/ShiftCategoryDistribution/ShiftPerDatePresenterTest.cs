using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftPerDatePresenterTest
	{
		private ShiftPerDatePresenter _presenter;
		private IShiftCategoryDistributionModel _model;
		private MockRepository _mock;
		private DateOnly _dateOnly;
		private IList<DateOnly> _dateOnlies;
		private IShiftCategory _shiftCategory;
		private IList<IShiftCategory> _shiftCategories;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_model = _mock.StrictMock<IShiftCategoryDistributionModel>();
			_presenter = new ShiftPerDatePresenter(_model);
			_dateOnly = new DateOnly(2013, 1, 1);
			_dateOnlies = new List<DateOnly> { _dateOnly };
			_shiftCategory = _mock.StrictMock<IShiftCategory>();
			_shiftCategories = new List<IShiftCategory>{_shiftCategory};
		}

		[Test]
		public void ShouldSetColHeader()
		{
			var style = new GridStyleInfo();
			var description = new Description("shiftCategory", "short");

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
				Expect.Call(_shiftCategory.Description).Return(description);
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 0, 1, null);
				Assert.AreEqual("short", style.CellValue);
				Assert.AreEqual(_shiftCategory, style.Tag);
			}
		}

		[Test]
		public void ShouldSetRowHeader()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(true)).Return(_dateOnlies);	
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				_presenter.SetCellInfo(style, 1, 0, null);
				Assert.AreEqual(_dateOnly.ToShortDateString(), style.CellValue);
			}
		}

		[Test]
		public void ShouldSetCountValues()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(true)).Return(_dateOnlies);
				Expect.Call(_model.ShouldUpdateViews).Return(true);
				Expect.Call(_model.ShiftCategoryCount(_dateOnly, _shiftCategory)).Return(14);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				_presenter.SetCellInfo(style, 1, 1, _shiftCategory);
				Assert.AreEqual(14, style.CellValue);
			}	
		}

		[Test]
		public void ShouldNotSetValuesWhenColAndRowEqualsZero()
		{
			var style = new GridStyleInfo {CellValue = 42};
			_presenter.SetCellInfo(style, 0, 0, null);
			Assert.AreEqual(42, style.CellValue);
		}

		[Test]
		public void ShouldNotSetValuesWhenRowIsGreaterThanSortedDates()
		{
			var style = new GridStyleInfo { CellValue = 42 };
			_presenter.SetCellInfo(style, 1, 0, null);
			Assert.AreEqual(42, style.CellValue);	
		}
		
		[Test]
		public void ShouldNotSetCountValuesWhenShouldNotUpdateView()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(true)).Return(_dateOnlies);
				Expect.Call(_model.ShouldUpdateViews).Return(false);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				_presenter.SetCellInfo(style, 1, 1, null);
			}
		}

		[Test]
		public void ShouldGetRowCount()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(true)).Return(_dateOnlies);	
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				var rowCount = _presenter.RowCount;
				Assert.AreEqual(1, rowCount);
			}
		}

		[Test]
		public void ShouldGetColCount()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
			}

			using (_mock.Playback())
			{
				var colCount = _presenter.ColumnCount();
				Assert.AreEqual(1, colCount);
			}
		}

		[Test]
		public void ShouldSortOnDates()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(true)).Return(_dateOnlies);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
			}
		}

		[Test]
		public void ShouldReverseSortOnDates()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedDates(false)).Return(_dateOnlies);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, false);
			}
		}

		[Test]
		public void ShouldSortOnNumberOfShiftCatgories()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetDatesSortedByNumberOfShiftCategories(_shiftCategory, true)).Return(_dateOnlies);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(_shiftCategory, false);
			}
		}

		[Test]
		public void ShouldReverseSortOnNumberOfShiftCategoriesWhenSortingOnSameShiftCatgoryTwice()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetDatesSortedByNumberOfShiftCategories(_shiftCategory, true)).Return(_dateOnlies);
				Expect.Call(_model.GetDatesSortedByNumberOfShiftCategories(_shiftCategory, false)).Return(_dateOnlies);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(_shiftCategory, false);
				_presenter.ReSort(_shiftCategory, false);
			}	
		}
	}
}
