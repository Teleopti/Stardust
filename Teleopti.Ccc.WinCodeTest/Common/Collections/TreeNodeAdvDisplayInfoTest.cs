using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Collections;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{

    [TestFixture]
    public class TreeNodeAdvDisplayInfoTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private TreeNodeAdvDisplayInfo _target;
        private ITreeItem<TreeNodeAdv> _treeItem;
        private TreeNodeAdv _treeNode;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _treeItem = new TreeItem<TreeNodeAdv>("ROOT");
            _treeNode = new TreeNodeAdv("Text");
            _treeItem.Node = _treeNode;
            _target = new TreeNodeAdvDisplayInfo(_treeNode);
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
            _treeItem = null;
            _treeNode = null;
        }

        #endregion


        #region Constructor Tests

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        #endregion

        #region Method Tests

        //[Test]
        //public void VerifyUpdateDisplayProperties()
        //{
        //    // Make method call
        //    _target.UpdateDisplayProperties(_treeItem);

        //    // Perform Assert Tests
        //    Assert.AreEqual(_treeNode.Checked, _target.IsChecked);
        //    Assert.AreEqual(_treeNode.IsSelected, _target.IsSelected);
        //    Assert.AreEqual(_treeNode.IsVisible, _target.IsVisible);
            

        //}


        #endregion

        #region Property Tests

        [Test]
        public void VerifyIsExpanded()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.IsExpanded = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue;

            // Test get method
            getValue = _target.IsExpanded;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyIsSelected()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.IsSelected = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue;

            // Test get method
            getValue = _target.IsSelected;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        #endregion

    }

}


