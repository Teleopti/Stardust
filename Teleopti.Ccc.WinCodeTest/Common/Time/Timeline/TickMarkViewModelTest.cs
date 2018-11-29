using System;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common.Time.Timeline
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TickMarkViewModelTest
    {
        private TickMarkViewModel _target;
        private DateTimePeriod _period;
        private DateTime _start;
        private TimeSpan _resolution;
       

        [SetUp]
        public void Setup()
        {
            _resolution = TimeSpan.FromMinutes(15);
            _start = new DateTime(2001,1,1,12,0,0,DateTimeKind.Utc);
            _period = new DateTimePeriod(_start,_start.AddHours(1));
            _target = new TickMarkViewModel(_period, _resolution);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(_target.MinorTickMarkHeight,10d);
            Assert.AreEqual(_target.MinimumTimeHeight,20d);
            Assert.AreEqual(_target.MajorTickMarkHeight,13d);
            Assert.AreEqual(_target.ShowTickMarks,true);
            Assert.AreEqual(_target.MinorTickMarkHeight,10d);
            Assert.AreEqual(_target.Resolution,_resolution);
            Assert.AreEqual(TickMarkViewModel.ResolutionProperty.DefaultMetadata.DefaultValue,TimeSpan.FromHours(1));
            Assert.AreEqual(_target.Period,_period);
        }

        [Test]
        public void VerifySetters()
        {
            double newVal = 1d;
            _target.ShowTickMarks = false;
            Assert.IsFalse(_target.ShowTickMarks);

            _target.MinorTickMarkHeight = newVal;
            Assert.AreEqual(_target.MinorTickMarkHeight, newVal);

            _target.MinimumTimeHeight = newVal;
            Assert.AreEqual(_target.MinimumTimeHeight,newVal);

            _target.MajorTickMarkHeight = newVal;
            Assert.AreEqual(_target.MajorTickMarkHeight,newVal);
        }

        [Test]
        public void VerifyPropertyChanged()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.Period = _period;
            Assert.IsFalse(listener.HasFired("Period"));
            _period = _period.MovePeriod(TimeSpan.FromMinutes(15));
            _target.Period = _period;
            Assert.AreEqual(_target.Period,_period);
            Assert.IsTrue(listener.HasOnlyFired("Period"));
        }
    }
}
