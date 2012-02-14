using System;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCodeTest.Common.ControlExtenders
{
    [TestFixture]
    public class TreeViewSmartSelectExtenderTest : IDisposable
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private TreeViewSmartSelectExtenderTestClass _target;
        private TreeView _treeView;
        private ITreeItem<TreeNode> _treeRoot;

        #endregion

        #region Test Preparation Methods

        /// <summary>
        /// Initialize test.
        /// </summary>
        /// <remarks>
        /// Runs before each test method.
        /// </remarks>
        [SetUp]
        public void TestInit()
        {
            // Initialize TreeViewExtender for the test suite
            _treeView = new TreeView();
            _treeRoot = new TreeItem<TreeNode>("Root");
            _target = new TreeViewSmartSelectExtenderTestClass(_treeView, _treeRoot, true, AutoParentSelectionOption.SelectParentIfAllChildSelected);
        }

        /// <summary>
        /// Finalize test.
        /// </summary>
        /// <remarks>
        /// Runs after each test method.
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            Dispose();
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Verifies the create tree view extender.
        /// </summary>
        [Test]
        public void VerifyCreateTreeViewExtender()
        {
            Assert.IsNotNull(_target);
        }

    

        #endregion

        #region Method Tests

        /// <summary>
        /// Verifies the branch check works.
        /// </summary>
        [Test]
        public void VerifySmartCheckBoxSelectSupportWorks()
        {
            ITreeItem<TreeNode> currentNode;
            
            _treeRoot.Node = _treeView.Nodes.Add("Root");

            currentNode = _treeRoot.AddChild("A");
            currentNode.Node = _treeView.Nodes[0].Nodes.Add("A");

            currentNode = _treeRoot.AddChild("B");
            currentNode.Node = _treeView.Nodes[0].Nodes.Add("B");

            currentNode = _treeRoot.FindItem("A", RangeOption.ThisAndRecursiveChildren).AddChild("A1");
            currentNode.Node = _treeView.Nodes[0].Nodes[0].Nodes.Add("A1");

            currentNode = _treeRoot.FindItem("A1", RangeOption.ThisAndRecursiveChildren).AddChild("A1a");
            currentNode.Node = _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1a");

            currentNode = _treeRoot.FindItem("A1", RangeOption.ThisAndRecursiveChildren).AddChild("A1b");
            currentNode.Node = _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1b");

            currentNode = _treeRoot.FindItem("A", RangeOption.ThisAndRecursiveChildren).AddChild("A2");
            currentNode.Node = _treeView.Nodes[0].Nodes[0].Nodes.Add("A2");

            currentNode = _treeRoot.FindItem("B", RangeOption.ThisAndRecursiveChildren).AddChild("B1");
            currentNode.Node = _treeView.Nodes[0].Nodes[1].Nodes.Add("B1");

            currentNode = _treeRoot.FindItem("B", RangeOption.ThisAndRecursiveChildren).AddChild("B2");
            currentNode.Node = _treeView.Nodes[0].Nodes[1].Nodes.Add("B2");

            currentNode = _treeRoot.FindItem("B1", RangeOption.ThisAndRecursiveChildren).AddChild("B1a");
            currentNode.Node = _treeView.Nodes[0].Nodes[1].Nodes[0].Nodes.Add("B1a");

            currentNode = _treeRoot.FindItem("B1", RangeOption.ThisAndRecursiveChildren).AddChild("B1b");
            currentNode.Node = _treeView.Nodes[0].Nodes[1].Nodes[0].Nodes.Add("B1b");

            _treeRoot.AutoChildSelection = true;
            _treeRoot.AutoParentSelection = AutoParentSelectionOption.SelectParentIfAllChildSelected;
            _target.SetTreeViewSelectStatus(_treeView.Nodes[0].Nodes[0]);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].NodeFont.Bold);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[1].NodeFont.Bold);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].NodeFont.Bold);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].Nodes[0].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].Nodes[1].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[1].NodeFont.Bold);

            _target.SetTreeViewSelectStatus(_treeView.Nodes[0].Nodes[0]);
            _target.SetTreeViewSelectStatus(_treeView.Nodes[0].Nodes[1]);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[1].Nodes[0].NodeFont.Bold);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[1].Nodes[1].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Nodes[0].NodeFont.Bold);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Nodes[1].NodeFont.Bold);

            //_treeView.Nodes[0].Nodes[0].Checked = true;
            //_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].NodeFont.Bold = false;
            //Assert.IsFalse(_treeView.Nodes[0].Nodes[0].NodeFont.Bold, "If a child is unchecked, the parent should be unchecked also.");
            //Assert.IsFalse(_treeView.Nodes[0].NodeFont.Bold);

            //_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked = true;
            //Assert.IsTrue(_treeView.Nodes[0].Nodes[0].NodeFont.Bold, "If all children are checked, the parent should be checked also.");

            //_treeRoot.AutoParentSelection = AutoParentSelectionOption.SelectParentIfAtLeastOneChildSelected;
            //_treeView.Nodes[0].Nodes[0].Checked = true;
            //_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked = false;
            //Assert.IsTrue(_treeView.Nodes[0].Nodes[0].NodeFont.Bold, "Only if all children are unchecked, the parent should be unchecked also.");
            //Assert.IsTrue(_treeView.Nodes[0].NodeFont.Bold);
            //_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Checked = false;
            //_treeView.Nodes[0].Nodes[0].Nodes[1].Checked = false;
            //Assert.IsFalse(_treeView.Nodes[0].Nodes[0].NodeFont.Bold, "If all children are unchecked, the parent should be unchecked also.");

        }

        #endregion

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">flag to make sure that some components are disposed only once.</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _target.Dispose();
                    _treeView.Dispose();
                }
            }
            disposed = true;
        }

        #endregion


    }
}