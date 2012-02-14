using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class WorkTimeLimitationAdapterTest
    {
      
        private TimePeriod _period;
        private TimeSpan _start;
        private TimeSpan _end;
        private MockRepository _mockRep;

        
        [SetUp]
        public void Setup()
        {
            _start = TimeSpan.FromHours(8);
            _end = TimeSpan.FromHours(11);
            _period = new TimePeriod(_start, _end);
            _mockRep = new MockRepository();
            
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            WorkTimeLimitationAdapter target = new WorkTimeLimitationAdapter(_period);
            Assert.AreEqual(_start,target.StartTime);
            Assert.AreEqual(_end,target.EndTime);
            Assert.AreEqual(_period,target.WorkTime);
        }

        [Test]
        public void TestPropertiesDoesNotSetToNull()
        {
            ILimitation limitation = _mockRep.StrictMock<ILimitation>();
            WorkTimeLimitationAdapter target = new WorkTimeLimitationAdapter(limitation);
           
            using (_mockRep.Record())
            {
                Expect.Call(limitation.StartTime = _start);
                Expect.Call(limitation.EndTime = _end);
            }
            using(_mockRep.Playback())
            {
                target.StartTime = null;
                target.StartTime = _start;
                target.EndTime = null;
                target.EndTime = _end;
            }
        }
        [Test]
        public void TestPropertiesCallsStringMethodsOnLimitation()
        {
            string test = "test";
            string timeString = "07:00";
            TimeSpan? timeSpan = null;
            ILimitation limitation = _mockRep.StrictMock<ILimitation>();
            WorkTimeLimitationAdapter target = new WorkTimeLimitationAdapter(limitation);

            using (_mockRep.Record())
            {
                Expect.Call(limitation.StartTimeString= test);
                Expect.Call(limitation.EndTimeString = test);
                Expect.Call(limitation.StartTimeString).Return(timeString);
                Expect.Call(limitation.EndTimeString).Return(timeString);
                Expect.Call(limitation.StringFromTimeSpan(timeSpan)).Return("nothing");
                Expect.Call(limitation.TimeSpanFromString(timeString)).Return(timeSpan);
                Expect.Call(limitation.StartTime).Return(TimeSpan.FromHours(7));
                Expect.Call(limitation.EndTime).Return(TimeSpan.FromHours(7));
            }
            using (_mockRep.Playback())
            {
                target.StartTimeString = test;
                target.EndTimeString = test;
                Assert.AreEqual(timeString,target.StartTimeString);
                Assert.AreEqual(timeString,target.EndTimeString);
                target.StringFromTimeSpan(timeSpan); //Expect call on underlying limitation
                target.TimeSpanFromString(timeString); //Expect call on underlying limitation
            }
        }

        [Test]
        public void VerifyTimeIsShownAsNothing()
        {
           _period = new TimePeriod(TimeSpan.Zero,TimeSpan.Zero);
           WorkTimeLimitationAdapter adapter = new WorkTimeLimitationAdapter(_period);

            Assert.AreEqual(string.Empty,adapter.StartTimeString);
            Assert.AreEqual(string.Empty,adapter.EndTimeString);
        }
    }
}