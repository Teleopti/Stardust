using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftDistributionGridPresenterTest
	{
        private ShiftDistributionGridPresenter _target;
        private MockRepository _mock;
        private IShiftDistributionGrid _view;
	    private ShiftDistribution _shiftDistribution1;
	    private ShiftDistribution _shiftDistribution2;
	    private IShiftCategory _morning;
	    private ShiftCategory _late;
	    private DateOnly _today;
	    private DateOnly _tomorrow;
	    private IDistributionInformationExtractor _extractor;
	    private List<ShiftDistribution> _shiftDistributionList;
	    private List<IShiftCategory> _shiftCategories;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
	        _morning = new ShiftCategory("Morning");
	        _late = new ShiftCategory("Late");
            _view = _mock.StrictMock<IShiftDistributionGrid>();
            _today = DateOnly.Today;
            _tomorrow = DateOnly.Today.AddDays(1);
            _shiftDistribution1 = new ShiftDistribution(_today , _morning, 5);
            _shiftDistribution2 = new ShiftDistribution(_tomorrow, _late, 3);

            _shiftDistributionList = new List<ShiftDistribution>
                {
                    _shiftDistribution1,
                    _shiftDistribution2
                };
            _target = new ShiftDistributionGridPresenter(_view,_shiftDistributionList  );
            _extractor = _mock.StrictMock<IDistributionInformationExtractor>();
	        _shiftCategories = new List<IShiftCategory> {_morning, _late};
        }

        [Test]
        public void TestIfCorrectCountIsReturned()
        {
            Assert.AreEqual(_target.ShiftCategoryCount(DateOnly.Today, _morning ),5);
        }

        [Test]
        public void TestIfNullIsReturnedIfNoShiftCategoryIsFound()
        {
            Assert.IsNull( _target.ShiftCategoryCount(DateOnly.Today, new ShiftCategory("Early")));
        }

        [Test]
        public void ShouldSortAndReSort()
        {
            var dates = new List<DateOnly> { _today, _tomorrow };

            using (_mock.Record())
            {
                Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.AtLeastOnce();
                Expect.Call(_extractor.GetShiftDistribution()).Return(_shiftDistributionList).Repeat.AtLeastOnce() ;
                Expect.Call(_extractor.Dates).Return(dates).Repeat.AtLeastOnce() ;
                Expect.Call(_extractor.ShiftCategories).Return(_shiftCategories).Repeat.AtLeastOnce();
            }
            using (_mock.Playback())
            {
                _target.Sort(0);
                Assert.AreEqual(_target.SortedDates()[0],_today );
                Assert.AreEqual(_target.SortedDates()[1],_tomorrow );

                _target.Sort(0);
                Assert.AreEqual(_target.SortedDates()[0], _tomorrow);
                Assert.AreEqual(_target.SortedDates()[1], _today);

                _target.Sort(1);
                Assert.AreEqual(_target.SortedDates()[0], _tomorrow );
                Assert.AreEqual(_target.SortedDates()[1], _today );

                _target.Sort(1);
                Assert.AreEqual(_target.SortedDates()[0], _today);
                Assert.AreEqual(_target.SortedDates()[1], _tomorrow);

                _target.ReSort();
                Assert.AreEqual(_target.SortedDates()[0], _today);
                Assert.AreEqual(_target.SortedDates()[1], _tomorrow);
            }
        }

        [Test]
        public void TestSortedDates()
        {
            var dates = new List<DateOnly> { _tomorrow, _today };

            using (_mock.Record())
            {
                Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.AtLeastOnce();
                Expect.Call(_extractor.Dates).Return(dates).Repeat.AtLeastOnce();
            }
            using (_mock.Playback())
            {
                var sortedDates =  _target.SortedDates() ;
                Assert.AreEqual(sortedDates[0], _today);
                Assert.AreEqual(sortedDates[1], _tomorrow);

                
            }
        }

        
	}
}
