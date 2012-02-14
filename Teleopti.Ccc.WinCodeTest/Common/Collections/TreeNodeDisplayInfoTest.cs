using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{

    [TestFixture]
    public class TreeNodeDisplayInfoTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private TreeNodeDisplayInfo _target;
        private ITreeItem<TreeNode> _treeItem;
        private TreeNode _treeNode;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _treeItem = new TreeItem<TreeNode>("ROOT");
            _treeNode = new TreeNode("Text");
            _treeItem.Node = _treeNode;
            _target = new TreeNodeDisplayInfo(_treeItem.Node);
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
        //    _target.UpdateDisplayProperties(_treeItem.Node);

        //    // Perform Assert Tests
        //    Assert.AreEqual(_treeNode.Checked, _target.IsChecked);
        //    Assert.AreEqual(_treeNode.IsExpanded, _target.IsExpanded);
        //    Assert.AreEqual(_treeNode.IsSelected, _target.IsSelected);
        //    Assert.AreEqual(_treeNode.IsVisible, _target.IsVisible);
        //    Assert.AreEqual(_treeNode.ImageIndex, _target.ImageIndex);
        //    Assert.AreEqual(_treeNode.ForeColor, _target.ForeColor);

        //}


        #endregion

        #region Property Tests

        [Test]
        public void VerifyIsVisible()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.IsVisible = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue;

            // Test get method
            getValue = _target.IsVisible;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

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
        public void VerifyIsChecked()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.IsChecked = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue;

            // Test get method
            getValue = _target.IsChecked;

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

        [Test]
        public void VerifyImageIndex()
        {
            // Declare variable to hold property set method
            System.Int32 setValue = 3;

            // Test set method
            _target.ImageIndex = setValue;

            // Declare return variable to hold property get method
            System.Int32 getValue;

            // Test get method
            getValue = _target.ImageIndex;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyForeColor()
        {
            // Declare variable to hold property set method
            Color setValue = Color.Yellow;

            // Test set method
            _target.ForeColor = setValue;

            // Declare return variable to hold property get method
            Color getValue;

            // Test get method
            getValue = _target.ForeColor;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        #endregion

    }

}


