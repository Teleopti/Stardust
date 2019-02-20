using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftStatisticsPresenterTest
	{
		private ShiftStatisticsPresenter _presenter;
		private IShiftCategoryDistributionModel _model;
		private MockRepository _mock;
		private IShiftCategory _shiftCategory;
		private IList<IShiftCategory> _shiftCategories;
		private IList<IPerson> _persons;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_model = _mock.StrictMock<IShiftCategoryDistributionModel>();
			_presenter = new ShiftStatisticsPresenter(_model);
			_shiftCategory = new ShiftCategory("shiftCategory");
			_shiftCategory.Description = new Description("shiftCatgory", "short");
			_shiftCategories = new List<IShiftCategory>{_shiftCategory};
			_persons = new List<IPerson> {new Person()};
		}

		[Test]
		public void ShouldNotSetValuesWhenColAndRowEqualsZero()
		{
			var style = new GridStyleInfo();
			style.CellValue = 32;
			_presenter.SetCellInfo(style, 0, 0, null);
			Assert.AreEqual(32, style.CellValue);
		}

		[Test]
		public void ShouldSetColumnHeaders()
		{
			var style = new GridStyleInfo();

			_presenter.SetCellInfo(style, 0, 1, null);
			Assert.AreEqual(UserTexts.Resources.Min, style.CellValue);

			_presenter.SetCellInfo(style, 0, 2, null);
			Assert.AreEqual(UserTexts.Resources.Max, style.CellValue);

			_presenter.SetCellInfo(style, 0, 3, null);
			Assert.AreEqual(UserTexts.Resources.Average, style.CellValue);

			//_presenter.SetCellInfo(style, 0, 4, null);
			//Assert.AreEqual(UserTexts.Resources.StandardDeviation, style.CellValue);
		}

		[Test]
		public void ShouldSetRowHeaders()
		{
			var style = new GridStyleInfo();
			

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 0, null);
				Assert.AreEqual("short", style.CellValue);

				//_presenter.SetCellInfo(style, 2, 0, null);
				//Assert.AreEqual(UserTexts.Resources.Total, style.CellValue);
			}
		}

		[Test]
		public void ShouldSortOnMin()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.GetShiftCategoriesSortedByMinMax(true, true)).Return(_shiftCategories);
				Expect.Call(_model.ShouldUpdateViews).Return(false);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(1, true);
				_presenter.SetCellInfo(style, 1, 1, null);
			}
		}

		[Test]
		public void ShouldSortOnMax()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.GetShiftCategoriesSortedByMinMax(true, false)).Return(_shiftCategories);
				Expect.Call(_model.ShouldUpdateViews).Return(false);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(2, true);
				_presenter.SetCellInfo(style, 1, 1, null);
			}
		}

		[Test]
		public void ShouldSortOnAverage()
		{
			var style = new GridStyleInfo();

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.GetShiftCategoriesSortedByAverage(false)).Return(_shiftCategories);
				Expect.Call(_model.ShouldUpdateViews).Return(false);
			}

			using (_mock.Playback())
			{
				_presenter.ReSort(3, false);
				_presenter.SetCellInfo(style, 1, 1, null);
			}	
		}

		//[Test]
		//public void ShouldSortOnStandardDeviation()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
		//		Expect.Call(_model.GetShiftCategoriesSortedByStandardDeviation(false)).Return(_shiftCategories);
		//		Expect.Call(_model.ShouldUpdateViews).Return(false);
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.ReSort(4, false);
		//		_presenter.SetCellInfo(style, 1, 1, null);
		//	}	
		//}

		//[Test]
		//public void ShouldSetCellTypeToIgnoreOnTotalRowAndNotOnTotalCell()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();	
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.SetCellInfo(style, 2, 3, null);
		//		Assert.AreEqual("IgnoreCellModel", style.CellType);
		//	}
		//}

		//[Test]
		//public void ShouldSetCellTypeToNumericReadOnlyCellOnTotalRowAndOnTotalCell()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
		//		Expect.Call(_model.ShouldUpdateViews).Return(true);
		//		Expect.Call(_model.GetSumOfDeviations()).Return(10d);
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.SetCellInfo(style, 2, 4, null);
		//		Assert.AreEqual("NumericReadOnlyCell", style.CellType);
		//	}		
		//}

		[Test]
		public void ShouldSetMinValue()
		{
			var style = new GridStyleInfo();
			var minMax = new MinMax<int>(5, 10);

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.ShouldUpdateViews).Return(true);
				Expect.Call(_model.GetMinMaxForShiftCategory(_shiftCategory)).Return(minMax);
				Expect.Call(() => _model.OnChartUpdateNeeded());
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 1, null);
				Assert.AreEqual(5, style.CellValue);
			}		
		}

		[Test]
		public void ShouldSetMaxValue()
		{
			var style = new GridStyleInfo();
			var minMax = new MinMax<int>(5, 10);

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.ShouldUpdateViews).Return(true);
				Expect.Call(_model.GetMinMaxForShiftCategory(_shiftCategory)).Return(minMax);
				Expect.Call(() => _model.OnChartUpdateNeeded());
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 2, null);
				Assert.AreEqual(10, style.CellValue);
			}		
		}

		[Test]
		public void ShouldSetAverageValue()
		{
			var style = new GridStyleInfo();
	
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
				Expect.Call(_model.ShouldUpdateViews).Return(true);
				Expect.Call(_model.GetSortedPersons(false)).Return(_persons);
				Expect.Call(_model.GetAverageForShiftCategory(_shiftCategory, _persons)).Return(23d);
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 3, null);
				Assert.AreEqual(23d, style.CellValue);
			}		
		}

		//[Test]
		//public void ShouldSetStandardDeviationValue()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories).Repeat.AtLeastOnce();
		//		Expect.Call(_model.ShouldUpdateViews).Return(true);
		//		Expect.Call(_model.GetStandardDeviationForShiftCategory(_shiftCategory)).Return(15d);
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.SetCellInfo(style, 1, 4, null);
		//		Assert.AreEqual(15d, style.CellValue);
		//	}	
		//}

		[Test]
		public void ShouldCauseUpdateChartWhenCellValueChangesOnCol1()
		{
			var style = new GridStyleInfo();
			var minMax = new MinMax<int>(10, 20);

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
				Expect.Call(_model.GetMinMaxForShiftCategory(_shiftCategory)).Return(minMax);
				Expect.Call(() => _model.OnChartUpdateNeeded());
				Expect.Call(_model.ShouldUpdateViews).Return(true);
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 1, null);		
			}	
		}

		[Test]
		public void ShouldCauseUpdateChartWhenCellValueChangesOnCol2()
		{
			var style = new GridStyleInfo();
			var minMax = new MinMax<int>(10, 20);

			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
				Expect.Call(_model.GetMinMaxForShiftCategory(_shiftCategory)).Return(minMax);
				Expect.Call(() => _model.OnChartUpdateNeeded());
				Expect.Call(_model.ShouldUpdateViews).Return(true);
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 2, null);
			}
		}

		[Test]
		public void ShouldNotSetValuesWhenShouldNotUpdateView()
		{
			var style = new GridStyleInfo();
			
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
				Expect.Call(_model.ShouldUpdateViews).Return(false);
			}

			using (_mock.Playback())
			{
				_presenter.SetCellInfo(style, 1, 2, null);
			}	
		}

		//[Test]
		//public void ShouldSetSumDeviationsWhenShouldUpdateView()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
		//		Expect.Call(_model.ShouldUpdateViews).Return(true);
		//		Expect.Call(_model.GetSumOfDeviations()).Return(10d);
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.SetCellInfo(style, 2, 4, null);
		//		Assert.AreEqual(10d, (double)style.CellValue);
		//	}		
		//}

		//[Test]
		//public void ShouldNotSetSumDeviationsWhenShouldNotUpdateView()
		//{
		//	var style = new GridStyleInfo();

		//	using (_mock.Record())
		//	{
		//		Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
		//		Expect.Call(_model.ShouldUpdateViews).Return(false);
		//	}

		//	using (_mock.Playback())
		//	{
		//		_presenter.SetCellInfo(style, 2, 4, null);
		//	}	
		//}

		[Test]
		public void ShouldGetRowCount()
		{
			using (_mock.Record())
			{
				Expect.Call(_model.GetSortedShiftCategories()).Return(_shiftCategories);
			}

			using (_mock.Playback())
			{
				var rowCount = _presenter.RowCount();
				Assert.AreEqual(1, rowCount);
			}
		}
	}
}
