using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    //Argh this one is broken, fix later
    [TestFixture]
    public class ShiftEditorSettingsTest
    {
        private ShiftEditorSettings _target;
        private TestTarget _testTarget;
        [SetUp]
        public void Setup()
        {
            _testTarget = new TestTarget();
            _target = new ShiftEditorSettings(_testTarget);
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(15),_target.Interval);
            Assert.AreEqual(false,_target.Expanded);
            Assert.AreEqual(true,_target.ClipAbsence);
            Assert.AreEqual(100d,_target.DetailsWidth);
        }

        [Test]
        public void VerifyCallsTargetWithNewSettings()
        {
            TimeSpan newInterval = _target.Interval.Add(TimeSpan.FromMinutes(1));
            bool newClipAbsence = !_target.ClipAbsence;
            bool newExpanded = !_target.Expanded;

            _target.Interval = newInterval;
            Assert.AreEqual(_testTarget.NewSettings.Interval,newInterval);

            _target.Expanded = newExpanded;
            Assert.AreEqual(_testTarget.NewSettings.Expanded, newExpanded);

            _target.ClipAbsence = newClipAbsence;
            Assert.AreEqual(_testTarget.NewSettings.ClipAbsence, newClipAbsence);

            Assert.AreEqual(3,_testTarget.NumberOfCalls,"Has been updated three times");
        }
        
        //[Test]
        //public void VerifyThatChangingTheShiftCategoryDoesNotCallSettingsAltered()
        //{
        //    //Henrik: Why?
        //    IShiftCategory category = new ShiftCategory("cat1");
        //   // _target.ShiftCategory = category;
        //   // Assert.AreEqual(category, _target.ShiftCategory, "Verify that its been selected");
        //    Assert.IsNull(_testTarget.NewSettings, "SettingsAltered is never called");
        //}

        [Test]
        public void VerifyThatWhenChangingTheDetailedWidthSettingTargetGetsUpdated()
        {
            double newWidth= _target.DetailsWidth + 3;
            _target.DetailsWidth = newWidth;
            Assert.AreEqual(newWidth,_testTarget.NewSettings.DetailsWidth);
            
        }

        [Test]
        public void VerifyIntervalIsMinimumOneMinute()
        {
            _target.Interval = TimeSpan.Zero;
            Assert.AreEqual(_target.Interval,TimeSpan.FromMinutes(1));
        }



        /// <summary>
        /// Grabs and exposes the newsettings, so we can see if and with what the target was called
        /// </summary>
        internal class TestTarget:IShiftEditorSettingsTarget
        {
            internal ShiftEditorSettings NewSettings { get; set; }
            internal int NumberOfCalls;
            
            public void SettingsAltered(ShiftEditorSettings settings)
            {
                NewSettings = settings;
                NumberOfCalls++;
            }

            public ShiftEditorSettings Settings
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
