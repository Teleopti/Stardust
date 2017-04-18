using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport;

namespace Teleopti.Ccc.WinCodeTest.FileImport
{
    [TestFixture]
    public class ImportFileDoTest
    {
        private ImportFileDo _target;

        [SetUp]
        public void Setup()
        {
            _target = ImportFileDo.Create("24;20090220;06:00;6;Queue 14;3;0;0;0;3;11;0;562;0;7;0;0;3;0;0;0", ";", 1);
        }

        [Test]
        public void VerifyData()
        {
            Assert.AreEqual(_target.Interval, "24");
            Assert.AreEqual(_target.Date, "20090220");
            Assert.AreEqual(_target.Time, "06:00");
            Assert.AreEqual(_target.Queue, "6");
            Assert.AreEqual(_target.QueueName, "Queue 14");
            Assert.AreEqual(_target.OfferedDirectCallCount, "3");
            Assert.AreEqual(_target.OverflowInCallCount, "0");
            Assert.AreEqual(_target.AbandonCallCount, "0");
            Assert.AreEqual(_target.OverflowOutcallCount, "0");
            Assert.AreEqual(_target.AnsweredCallCount, "3");
            Assert.AreEqual(_target.QueuedAndAnsweredCallDuration, "11");
            Assert.AreEqual(_target.QueuedAndAbandonCallDuration, "0");
            Assert.AreEqual(_target.TalkingCallDuration, "562");
            Assert.AreEqual(_target.WrapUpDuration, "0");
            Assert.AreEqual(_target.QueuedAnsweredLongestQueueDuration, "7");
            Assert.AreEqual(_target.QueuedAbandonLongestQueueDuration, "0");
            Assert.AreEqual(_target.AverageAvailMemberCount, "0");
            Assert.AreEqual(_target.AnsweredServiceLevelCount, "3");
            Assert.AreEqual(_target.WaitDuration, "0");
            Assert.AreEqual(_target.AbandonShortCallCount, "0");
            Assert.AreEqual(_target.AbandonWithinServiceLevelCount, "0");
        }

        [Test]
        public void VerifyCannotUseWrongFormatString()
        {
            Assert.Throws<FileImportException>(() => _target = ImportFileDo.Create("24;20090220;06:00;6", ";", 1));
        }
    }
}
