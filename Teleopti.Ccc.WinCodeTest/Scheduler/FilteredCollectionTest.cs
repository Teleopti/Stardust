using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using System.Drawing;
using System.Windows.Data;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class FilteredCollectionTest
    {
        FilteredCollection<ILayer<IActivity>> _collection;
        FakeLayer _fakeLayer;
        MainShiftActivityLayerNew _mainShiftActivityLayer;
        Activity _fakeActivity;
        DateTimePeriod _fakePeriod;

        [SetUp]
        public void Setup()
        { 
             _fakePeriod = new DateTimePeriod(2008,01,01,2008,01,03);
            _fakeActivity = ActivityFactory.CreateActivity("dummy", Color.DeepPink);
            _mainShiftActivityLayer = new MainShiftActivityLayerNew(_fakeActivity, _fakePeriod);

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
            _collection.AddFilter(typeof(MainShiftActivityLayerNew));
            _collection.AddFilter(typeof(MainShiftActivityLayerNew));
            Assert.AreEqual(1,_collection.Filters.Count);
        }

        [Test]
        public void VerifySetFilterClearsTheOldFilters()
        {
            _collection.AddFilter(typeof(MainShiftActivityLayerNew));
            _collection.AddFilter(typeof(PersonalShiftActivityLayer));
            _collection.SetFilter(typeof (AbsenceLayer));
            Assert.AreEqual(1,_collection.Filters.Count);
        }

        [Test]
        public void VerifyThatFilterRemovesFromCollectionView()
        {
            _collection.Add(_fakeLayer);
            _collection.Add(_mainShiftActivityLayer);
            Assert.AreEqual(1,CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            _collection.AddFilter(_fakeLayer.GetType());
            Assert.AreEqual(1, CollectionViewSource.GetDefaultView(_collection).OfType<MainShiftActivityLayerNew>().Count());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            _collection.RemoveFilter(_fakeLayer.GetType());
            Assert.AreEqual(1, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());

        }

        [Test]
        public void CanAddMultipleFilters()
        {
            _collection.Add(_fakeLayer);
            _collection.Add(_mainShiftActivityLayer);
            _collection.AddFilter(_fakeLayer.GetType());
            _collection.AddFilter(_mainShiftActivityLayer.GetType());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<FakeLayer>().Count());
            Assert.AreEqual(0, CollectionViewSource.GetDefaultView(_collection).OfType<MainShiftActivityLayerNew>().Count());
            Assert.AreEqual(2, _collection.Count);
        }

        private class FakeLayer : Layer<Activity>
        {
            public FakeLayer(Activity act, DateTimePeriod period)
                : base(act, period)
            {
            }

        }


    }




}
