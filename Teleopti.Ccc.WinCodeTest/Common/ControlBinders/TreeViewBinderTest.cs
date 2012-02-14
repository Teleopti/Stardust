using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlBinders;

namespace Teleopti.Ccc.WinCodeTest.Common.ControlBinders
{
    [TestFixture]
    public sealed class TreeViewBinderTest : IDisposable
    {

        #region Variables

        TreeViewBinder _treeViewBinder;
        TreeItem<TreeNode> _treeRoot;
        TreeItem<TreeNode> _treeItem1;
        TreeItem<TreeNode> _treeItem2;
        TreeView _treeView;

        #endregion

        #region Test Preparation Methods

        /// <summary>
        /// Setup test methods.
        /// </summary>
        [SetUp]
        public void TestInit()
        {
            _treeRoot = new TreeItem<TreeNode>("Root");
            _treeItem1 = new TreeItem<TreeNode>(_treeRoot, null, "Key", "Text1", "Tag", null);
            _treeItem2 = new TreeItem<TreeNode>(_treeRoot, null, "Key", "Text2", "Tag", null);
            _treeItem1.IsLeaf = true;
            _treeItem2.IsLeaf = false;
            _treeView = new TreeView();
            _treeViewBinder = new TreeViewBinder(_treeView, _treeRoot);
        }

        /// <summary>
        /// Disposes test methods.
        /// </summary>
        [TearDown]
        public void TestDispose()
        {
            this.Dispose();
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Tests constructor.
        /// </summary>
        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_treeViewBinder);
        }

        #endregion

        #region Method Tests

        /// <summary>
        /// Tests Display method.
        /// </summary>
        [Test]
        public void VerifyDisplay()
        {
            // Declare variables to pass to method call
            int expandLevel = 1;

            _treeViewBinder.SynchronizedWithTreeCollection = false;

            new TreeItem<TreeNode>(_treeItem1, "ITEM11");

            _treeViewBinder.SynchronizedWithTreeCollection = true;

            // Make method call
            _treeViewBinder.Display(expandLevel);

            // SyncronizedWithTreeCollection property automatically must be changed to True
            Assert.IsTrue(_treeViewBinder.SynchronizedWithTreeCollection);

            Assert.AreEqual(_treeView.Nodes.Count, 1);
            Assert.AreEqual(_treeView.Nodes[0].Text, "Root");
            Assert.IsTrue(_treeView.Nodes[0].IsExpanded);
            Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 2);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].IsExpanded);
            Assert.AreEqual(_treeView.Nodes[0].Nodes[0].Text, "Text1");

            //// change some properties
            //_treeView.Nodes[0].Nodes[0].Expand();

            //// save displayinfo
            ////_treeRoot.SaveNodeDisplayInfo();

            //// Make method call
            //_treeViewBinder.Display(expandLevel);

            //Assert.IsTrue(_treeView.Nodes[0].IsExpanded);
            //Assert.IsTrue(_treeView.Nodes[0].Nodes[0].IsExpanded);

        }

        ///// <summary>
        ///// Verifies the GetChecked method.
        ///// </summary>
        //[Test]
        //public void VerifyGetChecked()
        //{
        //    _treeView.CheckBoxes = true;

        //    // Make method call
        //    _treeViewBinder.Display(3);

        //    // set some nodes checked
        //    _treeView.Nodes[0].Nodes[1].Checked = true;

        //    ReadOnlyCollection<ITreeItem<TreeNode>> checkedItems = _treeViewBinder.CheckedItemCollection;

        //    Assert.AreEqual(1, checkedItems.Count);

        //    Assert.AreSame(_treeItem2, checkedItems[0]);

        //}

        ///// <summary>
        ///// Verifies that the get checked throws exception if checked is not set.
        ///// </summary>
        //[Test]
        //[ExpectedException(typeof(NotSupportedException))]
        //public void VerifyGetCheckedThrowsExceptionIfCheckedIsNotSet()
        //{
        //    // Make method call
        //    _treeViewBinder.Display(3);

        //    // set some nodes checked
        //    _treeView.Nodes[0].Nodes[1].Checked = true;

        //    ReadOnlyCollection<ITreeItem<TreeNode>> getChecked = _treeViewBinder.CheckedItemCollection;
        //}

        /// <summary>
        ///  Tests Display method when the viewer is synconised 
        /// </summary>
        [Test]
        public void VerifyDisplaySynchronized()
        {
            // Declare variables to pass to method call
            int expandLevel = 3;
            ITreeItem<TreeNode> _runTimeItem1;
            ITreeItem<TreeNode> _runTimeItem2;

            // Make method call
            _treeViewBinder.Display(expandLevel);

            _runTimeItem1 = _treeRoot.AddChild(new TreeItem<TreeNode>("RunTime1"));

            _runTimeItem2 = new TreeItem<TreeNode>("RunTime2");
            _runTimeItem2.IsLeaf = true;
            _treeRoot.AddChild(_runTimeItem2);

            Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 4);
            Assert.AreEqual(_treeView.Nodes[0].Nodes[3].Text, "RunTime2");

            _treeRoot.RemoveChild(_runTimeItem1);
            _treeRoot.RemoveChild(_runTimeItem2);
            Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 2);
        }

        [Test]
        public void VerifySaveDisplayInformation()
        {
            
            // test nodeinfo with no node
            _treeRoot.Node = new TreeNode("ROOT");
            _treeItem1.Node = new TreeNode("ITEM1");
            _treeItem2.Node = new TreeNode("ITEM2");

            Assert.IsNull(_treeRoot.NodeDisplayInfo);
            Assert.IsNull(_treeItem1.NodeDisplayInfo);
            Assert.IsNull(_treeItem2.NodeDisplayInfo);

            TreeViewBinder.SaveDisplayInformation(_treeRoot);

            Assert.IsNotNull(_treeRoot.NodeDisplayInfo);
            Assert.IsNotNull(_treeItem1.NodeDisplayInfo);
            Assert.IsNotNull(_treeItem2.NodeDisplayInfo);

        }

        [Test]
        public void VerifySynchronizeDisplayInformation()
        {

            // test nodeinfo with no node
            TreeItem<TreeNode> oldRoot = new TreeItem<TreeNode>("KEYROOT", "OLDROOT");
            TreeItem<TreeNode> oldItem1 = new TreeItem<TreeNode>(oldRoot, "KEYITEM1", "OLDITEM1");
            TreeItem<TreeNode> oldItem2 = new TreeItem<TreeNode>(oldRoot, "KEYITEM2", "OLDITEM2");

            oldRoot.Node = new TreeNode("ROOT");
            oldItem1.Node = new TreeNode("ITEM1");
            oldItem2.Node = new TreeNode("ITEM2");

            TreeItem<TreeNode> newRoot = new TreeItem<TreeNode>("KEYROOT", "NEWROOT");
            TreeItem<TreeNode> newItem1 = new TreeItem<TreeNode>(newRoot, "KEYITEM1", "NEWITEM1");
            TreeItem<TreeNode> newItem2 = new TreeItem<TreeNode>(newRoot, "KEYITEM2", "NEWITEM2");

            Assert.IsNull(newRoot.NodeDisplayInfo);
            Assert.IsNull(newItem1.NodeDisplayInfo);
            Assert.IsNull(newItem2.NodeDisplayInfo);

            _treeViewBinder = new TreeViewBinder(new TreeView(), newRoot);

            _treeViewBinder.SynchronizeDisplayInformation(oldRoot);

            Assert.IsNotNull(newRoot.NodeDisplayInfo);
            Assert.IsNotNull(newItem1.NodeDisplayInfo);
            Assert.IsNotNull(newItem2.NodeDisplayInfo);
        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Tests Settings property
        /// </summary>
        [Test]
        public void Settings()
        {
            // Declare return variable to hold property get method
            bool _getValue;
            bool _setValue = true;

            // set value first
            _treeViewBinder.SynchronizedWithTreeCollection = _setValue;

            // Test get method
            _getValue = _treeViewBinder.SynchronizedWithTreeCollection;

            // Perform Assert Tests
            Assert.AreEqual(_getValue, _setValue);
        }

        /// <summary>
        ///  Tests TreeControl property
        /// </summary>
        [Test]
        public void TreeControl()
        {
            // Declare return variable to hold property get method
            object _getValue;

            // Test get method
            _getValue = _treeViewBinder.TreeControl;

            Assert.AreSame(_treeView, _getValue);
        }

        /// <summary>
        ///  Tests TreeView property
        /// </summary>
        [Test]
        public void TreeView()
        {
            // Declare return variable to hold property get method
            TreeView _getValue;
            TreeView _setValue;

            // Test set method
            _setValue = new TreeView();
            _treeViewBinder.TreeView = _setValue;

            // Test get method
            _getValue = _treeViewBinder.TreeView;

            Assert.AreSame(_setValue, _getValue);
        }

        /// <summary>
        /// Tests RootItem property.
        /// </summary>
        [Test]
        public void RootItem()
        {
            // Declare return variable to hold property get method
            ITreeItem<TreeNode> _getValue;
            ITreeItem<TreeNode> _setValue;

            // Test set method
            _setValue = new TreeItem<TreeNode>("NewRoot");
            _treeViewBinder.RootItem = _setValue;

            // Test get method
            _getValue = _treeViewBinder.RootItem;


            // Perform Assert Tests
            Assert.AreSame(_getValue, _setValue);
        }

        #endregion

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_treeView")]
        public void Dispose()
        {
            _treeViewBinder.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}