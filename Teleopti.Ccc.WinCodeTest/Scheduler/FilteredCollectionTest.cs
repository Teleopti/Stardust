using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using System.Drawing;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class FilteredCollectionTest
    {
        FilteredCollection<ILayer<IActivity>> _collection;
        FakeLayer _fakeLayer;
        MainShiftLayer _mainShiftLayer;
        IActivity _fakeActivity;
        DateTimePeriod _fakePeriod;

        [SetUp]
        public void Setup()
        { 
             _fakePeriod = new DateTimePeriod(2008,01,01,2008,01,03);
            _fakeActivity = ActivityFactory.CreateActivity("dummy", Color.DeepPink);
            _mainShiftLayer = new MainShiftLayer(_fakeActivity, _fakePeriod);

            _fakeLayer = new FakeLayer(_fakeActivity, _fakePeriod);
            _collection = new FilteredCollection<ILayer<IActivity>>();
        }

        [Test]
        public void CanCreateEmptyFilteredCollection()
        {
            Assert.AreEqual(0, _collection.Count);
        }



        [Test]
        public void CannotAddSameFilterTwice()
        {
            _collection.AddFilter(typeof(MainShiftLayer));
            _collection.AddFilter(typeof(MainShiftLayer));
            Assert.AreEqual(1,_collection.Filters.Count);
        }

        [Test]
        public void VerifySetFilterClearsTheOldFilters()
        {
            _collection.AddFilter(typeof(MainShiftLayer));
            _collection.AddFilter(typeof(PersonalShiftLayer));
            _collection.SetFilter(typeof (AbsenceLayer));
            Assert.AreEqual(1,_collection.Filters.Count);
        }

        [Test]
        public void VerifyThatFilterRemovesFromCollectionView()
        {
            _collection.Add(_fakeLayer);
            _collection.Add(_mainShiftLayer);
            Assert.AreEqual(1,CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            _collection.AddFilter(_fakeLayer.GetType());
            Assert.AreEqual(1, CollectionViewSource.GetDefaultView(_collection).OfType<MainShiftLayer>().Count());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            _collection.RemoveFilter(_fakeLayer.GetType());
            Assert.AreEqual(1, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());

        }

        [Test]
        public void CanAddMultipleFilters()
        {
            _collection.Add(_fakeLayer);
            _collection.Add(_mainShiftLayer);
            _collection.AddFilter(_fakeLayer.GetType());
            _collection.AddFilter(_mainShiftLayer.GetType());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<MainShiftLayer>().Count());
            Assert.AreEqual(2, _collection.Count);
        }

        private class FakeLayer : Layer<IActivity>
        {
            public FakeLayer(IActivity act, DateTimePeriod period)
                : base(act, period)
            {
            }

        }


    }




}
