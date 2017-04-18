using System.Windows.Data;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Interop;

namespace Teleopti.Ccc.WinCodeTest.Common.Interop
{
    [TestFixture]
    public class MultipleHostViewModelTest
    {
        private IMultipleHostViewModel _target;
        private string _header;
        private string _content;

        [SetUp]
        public void Setup()
        {
            _target = new MultipleHostViewModel();
            _header = "header";
            _content = "content";
        }
	
        [Test]
        public void CanCreateMultipleHostViewModelWithChildren()
        {
          
            _target.Add(_header, _content);

            //CollectionViewSource.GetDefaultView(_target.Items).MoveCurrentTo()
            Assert.AreEqual(1,_target.Items.Count);
            Assert.AreEqual(_header,_target.CurrentHeader);
            Assert.AreEqual(_content, _target.CurrentItem);
        }

        [Test]
        public void VerifyCannotShowNullIfModelsExists()
        {
            Assert.IsNull(_target.CurrentHeader);
            Assert.IsNull(_target.CurrentItem);
            _target.Add(_header,_content);
            CollectionViewSource.GetDefaultView(_target.Items).MoveCurrentToPosition(-1);
            Assert.AreEqual(_header, _target.CurrentHeader);
            Assert.AreEqual(_content, _target.CurrentItem);
        }
    }
}
