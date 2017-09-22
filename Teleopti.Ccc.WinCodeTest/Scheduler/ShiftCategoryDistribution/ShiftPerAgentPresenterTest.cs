using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftPerAgentPresenterTest
	{
		private ShiftPerAgentPresenter _presenter;
		private MockRepository _mock;
		private IShiftCategoryDistributionModel _model;
		private IPerson _person;
		private IList<IPerson> _persons;
		private IShiftCategory _shiftCategory;
		private IList<IShiftCategory> _shiftCategories;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_model = _mock.StrictMock<IShiftCategoryDistributionModel>();
			_presenter = new ShiftPerAgentPresenter(_model);
			_person = _mock.StrictMock<IPerson>();
			_persons = new List<IPerson> { _person };
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
				_presenter.SetCellInfo(style,0,1,null);
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
				Expect.Call(_model.GetSortedPersons(true)).Return(_persons);
				Expect.Call(_model.CommonAgentName(_person)).Return("person");
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				_presenter.SetCellInfo(style, 1, 0, null);
				Assert.AreEqual("person", style.CellValue);
			}
		}

		[Test]
		public void ShouldSetCountValue()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.ShouldUpdateViews).Return(true);
				Expect.Call(_model.GetSortedPersons(true)).Return(_persons);
				Expect.Call(_model.ShiftCategoryCount(_person, _shiftCategory)).Return(12);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
				_presenter.SetCellInfo(style,1, 1, _shiftCategory);
				Assert.AreEqual(12, style.CellValue);
			}
		}

		[Test]
		public void ShouldNotSetValueWhenColAndRowEqualsZero()
		{
			var style = new GridStyleInfo {CellValue = 42};
			_presenter.SetCellInfo(style, 0,0, null);
			Assert.AreEqual(42, style.CellValue);
		}

		[Test]
		public void ShouldNotSetValueWhenRowGreaterThanSortedPersons()
		{
			var style = new GridStyleInfo { CellValue = 42 };
			_presenter.SetCellInfo(style,1,0,null);
			Assert.AreEqual(42, style.CellValue);
		}

		[Test]
		public void ShouldNotSetCountValueWhenShouldNotUpdate()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedPersons(true)).Return(_persons);
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
				Expect.Call(_model.GetSortedPersons(true)).Return(_persons);
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
		public void ShouldSortOnPersonsKeepOrder()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedPersons(true));
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, true);
			}
		}

		[Test]
		public void ShouldSortOnPersonsReversOrder()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedPersons(false));
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(null, false);
			}	
		}

		[Test]
		public void ShouldSortOnNumberOfShiftCategories()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetAgentsSortedByNumberOfShiftCategories(_shiftCategory, true)).Return(_persons);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(_shiftCategory, false);
			}
		}

		[Test]
		public void ShouldReverseSortOnNumberOfShiftCategoriesWhenSortingOnSameShiftCategoryTwice()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetAgentsSortedByNumberOfShiftCategories(_shiftCategory, true)).Return(_persons);
				Expect.Call(_model.GetAgentsSortedByNumberOfShiftCategories(_shiftCategory, false)).Return(_persons);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(_shiftCategory, false);
				_presenter.ReSort(_shiftCategory, false);
			}	
		}
	}
}
