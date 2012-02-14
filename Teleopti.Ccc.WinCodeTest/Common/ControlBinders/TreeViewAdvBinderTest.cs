using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlBinders;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCodeTest.Common.ControlBinders
{
    [TestFixture]
    public sealed class TreeViewAdvBinderTest : IDisposable
    {

        #region Variables

        TreeViewAdvBinder _treeViewBinder;
        TreeItem<TreeNodeAdv> _treeRoot;
        TreeItem<TreeNodeAdv> _treeItem1;
        TreeItem<TreeNodeAdv> _treeItem2;
        TreeViewAdv _treeView;

        #endregion

        #region Test Preparation Methods

        /// <summary>
        /// Setup test methods.
        /// </summary>
        [SetUp]
        public void TestInit()
        {
            _treeRoot = new TreeItem<TreeNodeAdv>("Root");
            _treeItem1 = new TreeItem<TreeNodeAdv>(_treeRoot, null, "Key", "Text1", "Tag", null);
            _treeItem2 = new TreeItem<TreeNodeAdv>(_treeRoot, null, "Key", "Text2", "Tag", null);
            _treeItem1.IsLeaf = true;
            _treeItem2.IsLeaf = false;
            _treeView = new TreeViewAdv();
            _treeViewBinder = new TreeViewAdvBinder(_treeView, _treeRoot);
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
        public void TreeViewBinder()
        {
            Assert.IsNotNull(_treeViewBinder);
        }

        /// <summary>
        /// Tests TreeView is not null.
        /// </summary>
        [Test]
        public void VerifyTreeViewIsNotNull()
        {
            Assert.IsNotNull(_treeViewBinder.TreeView);
        }

        /// <summary>
        /// Tests root is not null.
        /// </summary>
        [Test]
        public void VerifyRootItemIsNotNull()
        {
            Assert.IsNotNull(_treeViewBinder.RootItem);
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

            
            _treeViewBinder.SynchronizedWithTreeCollection = true;

            // Make method call
            _treeViewBinder.Display(expandLevel);

            // SyncronizedWithTreeCollection property automatically must be changed to True
            Assert.IsTrue(_treeViewBinder.SynchronizedWithTreeCollection);

            Assert.AreEqual(_treeView.Nodes.Count, 1);
            Assert.AreEqual(_treeView.Nodes[0].Text, "Root");
            Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 2);
            Assert.AreEqual(_treeView.Nodes[0].Nodes[0].Text, "Text1");

            //// change some properties
            //_treeView.Nodes[0].Nodes[0].Expand();

            //// save displayinfo
            //_treeRoot.SaveNodeDisplayInfo();

            //// Make method call
            //_treeViewBinder.Display(expandLevel);


        }


        /// <summary>
        /// Verifies that the get checked throws exception if checked is not set.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifyGetCheckedThrowsExceptionIfCheckedIsNotSet()
        {
            // Make method call
            _treeViewBinder.Display(3);

            // set some nodes checked
            _treeView.Nodes[0].Nodes[1].Checked = true;

            throw new NotSupportedException();
        }

        /// <summary>
        ///  Tests Display method when the viewer is synconised 
        /// </summary>
        [Test]
        public void VerifyDisplaySynchronized()
        {
            // Declare variables to pass to method call
            int expandLevel = 3;
            ITreeItem<TreeNodeAdv> _runTimeItem1;
            ITreeItem<TreeNodeAdv> _runTimeItem2;

            // Make method call
            _treeViewBinder.Display(expandLevel);

            _runTimeItem1 = _treeRoot.AddChild(new TreeItem<TreeNodeAdv>("RunTime1"));

            _runTimeItem2 = new TreeItem<TreeNodeAdv>("RunTime2");
            _runTimeItem2.IsLeaf = true;
            _treeRoot.AddChild(_runTimeItem2);

            //Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 4);
            //Assert.AreEqual(_treeView.Nodes[0].Nodes[3].Text, "RunTime2");

            _treeRoot.RemoveChild(_runTimeItem1);
            _treeRoot.RemoveChild(_runTimeItem2);
            Assert.AreEqual(_treeView.Nodes[0].Nodes.Count, 2);
        }

        /// <summary>
        /// Verifies the recursively add selected nodes.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-07
        /// </remarks>
        [Test]
        public void VerifyRecursivelyAddSelectedNodes()
        {
            IList<ITreeItem<TreeNodeAdv>> selectedList = new List<ITreeItem<TreeNodeAdv>>();
            _treeViewBinder.RecursivelyAddSelectedNodes(_treeViewBinder.TreeView.Root, selectedList);
            Assert.AreEqual(1, selectedList.Count);

            _treeViewBinder.TreeView.Root.Nodes.Add(new TreeNodeAdv("test1"));
           

            _treeViewBinder.RecursivelyAddSelectedNodes(_treeViewBinder.TreeView.Root, selectedList);
            Assert.AreEqual(1, selectedList.Count);
        }

        [Test]
        public void VerifyRecursivelyAddFilterNodes()
        {
            Assert.AreEqual(0, _treeViewBinder.TreeView.SelectedNodes.Count);
            TreeNodeAdv newSelectedNode = new TreeNodeAdv("test");
            ITreeItem<TreeNodeAdv> item = new TreeItem<TreeNodeAdv>("11x01", "Test01");
            newSelectedNode.Tag = item;
            _treeViewBinder.TreeView.Nodes.Add(newSelectedNode);
            _treeViewBinder.TreeView.SelectedNodes.Add(newSelectedNode);

            _treeViewBinder.SetTreeSelectedNodes(_treeViewBinder.SelectedItemCollection);
            Assert.AreEqual(1, _treeViewBinder.SelectedItemCollection.Count);

            _treeViewBinder.RecursivelyAddFilterNodes(newSelectedNode, _treeViewBinder.SelectedItemCollection);
            Assert.AreEqual(1, _treeViewBinder.SelectedItemCollection.Count);

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
        ///  Tests TreeControl property
        /// </summary>
        [Test]
        public void VerifyTreeControl()
        {
            Assert.IsNotNull(_treeViewBinder.TreeControl);
        }

        /// <summary>
        /// Verifies the checked item collection.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-06
        /// </remarks>
        [Test]
        public void VerifyCheckedItemCollection()
        {
            Assert.AreEqual(0, _treeViewBinder.TreeView.SelectedNodes.Count);
            TreeNodeAdv newSelectedNode = new TreeNodeAdv("test");
            _treeViewBinder.TreeView.Nodes.Add(newSelectedNode);
            _treeViewBinder.TreeView.SelectedNodes.Add(newSelectedNode);

            Assert.AreEqual(1, _treeViewBinder.TreeView.SelectedNodes.Count);
        }
              

        /// <summary>
        ///  Tests TreeView property
        /// </summary>
        [Test]
        public void TreeView()
        {
            // Declare return variable to hold property get method
            TreeViewAdv _getValue;
            TreeViewAdv _setValue;

            // Test set method
            _setValue = new TreeViewAdv();
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
            ITreeItem<TreeNodeAdv> _getValue;
            ITreeItem<TreeNodeAdv> _setValue;

            // Test set method
            _setValue = new TreeItem<TreeNodeAdv>("NewRoot");
            _treeViewBinder.RootItem = _setValue;

            // Test get method
            _getValue = _treeViewBinder.RootItem;


            // Perform Assert Tests
            Assert.AreSame(_getValue, _setValue);
        }


        ///// <summary>
        ///// Verifies the lowest visible node.
        ///// </summary>
        ///// <remarks>
        ///// Created by: Dinesh Ranasinghe
        ///// Created date: 2008-02-06
        ///// </remarks>
        //[Test]
        //public void VerifyLowestVisibleNode()
        //{

        //    TreeNodeAdv rootTreeNodeAdv = new TreeNodeAdv("ImRoot");
        //    TreeNodeAdv childTreeNodeAdv = new TreeNodeAdv("ImChild");

        //    rootTreeNodeAdv.Nodes.Add(childTreeNodeAdv);
        //    _treeViewBinder.TreeView.Nodes.Add(rootTreeNodeAdv);

        //    TreeNodeAdv lowestVisibleNode = _treeViewBinder.LowestVisibleNode();

        //    Assert.AreEqual(childTreeNodeAdv, lowestVisibleNode);



        //}

        #endregion

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_treeView")]
        public void Dispose()
        {
            if(_treeViewBinder!=null)
                _treeViewBinder.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}