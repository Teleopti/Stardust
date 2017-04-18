using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
    [TestFixture]
    public class ClipHandlerTest
    {
        ClipHandler<string> _clipHandler;
        string _stringObject;

        [SetUp]
        public void Setup()
        {
            _clipHandler = new ClipHandler<string>();
            _stringObject = "test";
        }

        

        [Test]
        public void CanCreateClipHandler()
        {
            Assert.IsNotNull(_clipHandler);
        }

        [Test]
        public void CanAddAndClearClip()
        {
            _clipHandler.AddClip(10, 10, _stringObject);
            Assert.IsNotEmpty(_clipHandler.ClipList);

            _clipHandler.Clear();
            Assert.IsEmpty(_clipHandler.ClipList);

        }

        [Test]
        public void CheckRSpaAndCSpa()
        {
            _clipHandler.AddClip(1, 1, _stringObject);
            _clipHandler.AddClip(3, 3, _stringObject);

            Assert.AreEqual(3, _clipHandler.RowSpan());
            Assert.AreEqual(3, _clipHandler.ColSpan());
        }

        [Test]
        public void CanGetSetClipMode()
        {
            _clipHandler.IsInCutMode = true;
            Assert.IsTrue(_clipHandler.IsInCutMode);

            PasteOptions deleteOptions = new PasteOptions();
            _clipHandler.CutMode = deleteOptions;

            Assert.AreEqual(deleteOptions, _clipHandler.CutMode);
        }

        [Test]
        public void CanGetEmptyClipBounds()
        {
            Assert.AreEqual(1, _clipHandler.ColSpan());
            Assert.AreEqual(1, _clipHandler.RowSpan());
        }
    }
}
