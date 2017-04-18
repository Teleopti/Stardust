using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Interop;

namespace Teleopti.Ccc.WinCodeTest.Common.Interop
{
    [TestFixture]
    public class HostViewModelTest
    {
        private HostViewModel _target;
        private object _header;
        private object _content;

        [SetUp]
        public void Setup()
        {
            _header = "header";
            _content = "content";
            _target = new HostViewModel(_header,_content);
        }

        [Test]
        public void VerifyHeaderAndContentProperties()
        {
            Assert.AreEqual(_target.ModelHeader, _header);
            Assert.AreEqual(_target.ModelContent, _content);
            Assert.AreEqual(HostViewModel.ModelContentProperty.DefaultMetadata.DefaultValue,string.Empty);
            Assert.AreEqual(HostViewModel.ModelHeaderProperty.DefaultMetadata.DefaultValue,string.Empty);
			_target.UpdateItem("huvud", "innehåll");
			Assert.AreEqual(_target.ModelHeader, "huvud");
			Assert.AreEqual(_target.ModelContent, "innehåll");
        }

    }
}
