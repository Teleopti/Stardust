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

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
	        _morning = new ShiftCategory("Morning");
	        _late = new ShiftCategory("Late");
            _view = _mock.StrictMock<IShiftDistributionGrid>();
            _shiftDistribution1 = new ShiftDistribution(DateOnly.Today,_morning,5 );
            _shiftDistribution2 = new ShiftDistribution(DateOnly.Today,_late, 3 );

            IList<ShiftDistribution> shiftDistributionList = new List<ShiftDistribution>
                {
                    _shiftDistribution1,
                    _shiftDistribution2
                };
            _target = new ShiftDistributionGridPresenter(_view,shiftDistributionList  );
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
	}
}
