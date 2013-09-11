using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftFairnessGridPresenterTest
	{
		private ShiftFairnessGridPresenter _target;
		private MockRepository _mock;
		private IShiftFairnessGrid _view;
		private IDistributionInformationExtractor _extractor;
		

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IShiftFairnessGrid>();
			_target = new ShiftFairnessGridPresenter(_view);
			_extractor = _mock.StrictMock<IDistributionInformationExtractor>();
		}

		[Test]
		public void ShouldReturnColCount()
		{
			Assert.AreEqual(4, _target.ColCount);
		}

		[Test]
		public void ShouldCalculateTotalStandardDeviation()
		{
			var shiftCategory1 = new ShiftCategory("shiftCategory1");
			var shiftCategory2 = new ShiftCategory("shiftCategory2");
			var fairness1 = new ShiftFairness(shiftCategory1, 2, 4, 3, 1.0f);
			var fairness2 = new ShiftFairness(shiftCategory2, 2, 4, 3, 1.0f);
			var fairnessList = new List<ShiftFairness> {fairness1, fairness2};

			var total = _target.CalculateTotalStandardDeviation(fairnessList);
			Assert.AreEqual(2.0f, total);
		}

		[Test]
		public void ShouldSort()
		{
			var shiftCategory1 = new ShiftCategory("A");
			var shiftCategory2 = new ShiftCategory("B");
			var fairness1 = new ShiftFairness(shiftCategory1, 2, 4, 3, 1.0f);
			var fairness2 = new ShiftFairness(shiftCategory2, 3, 7, 5, 2.0f);
			var fairnessList = new List<ShiftFairness> { fairness1, fairness2 };
			var shiftCategories = new List<IShiftCategory> {shiftCategory1, shiftCategory2};

			using (_mock.Record())
			{
				Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftFairness ).Return(fairnessList).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftCategories).Return(shiftCategories).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				_target.Sort(0);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory1.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory2.Description);

				_target.Sort(0);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);

				_target.Sort(1);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory1.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory2.Description);

				_target.Sort(1);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);

				_target.Sort(2);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory1.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory2.Description);

				_target.Sort(2);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);
				
				_target.Sort(3);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory1.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory2.Description);

				_target.Sort(3);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);
				
				_target.Sort(4);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory1.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory2.Description);

				_target.Sort(4);
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);

				_target.ReSort();
				Assert.AreEqual(_target.SortedShiftCategories()[0].Description, shiftCategory2.Description);
				Assert.AreEqual(_target.SortedShiftCategories()[1].Description, shiftCategory1.Description);
			}	
		}
	}
}
