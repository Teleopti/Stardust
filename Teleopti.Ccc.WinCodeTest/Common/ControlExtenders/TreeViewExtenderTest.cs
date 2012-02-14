using System;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.ControlExtenders;

namespace Teleopti.Ccc.WinCodeTest.Common.ControlExtenders
{
    [TestFixture]
    public class TreeViewExtenderTest : IDisposable
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private TreeViewExtender _target;
        private TreeView _treeView;

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
            _target = new TreeViewExtender(_treeView);
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

        /// <summary>
        /// Verifies that can not created tree view extender with null as TreeView.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyExceptionThrownWhenCreateTreeViewExtenderWithNull()
        {
            _target = new TreeViewExtender(null);
        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifySmartParentSelectingType()
        {
            SmartParentSelectingOption _setValue = SmartParentSelectingOption.SelectParentIfAtLeastOneChildSelected;
            _target.SmartParentSelectingType = _setValue;

            SmartParentSelectingOption _getValue;
            _getValue = _target.SmartParentSelectingType;

            Assert.AreEqual(_setValue, _getValue);
        
        }

        [Test]
        public void VerifySmartChildSelectingType()
        {
            SmartChildSelectingOption _setValue = SmartChildSelectingOption.SelectChildrenRecursively;
            _target.SmartChildSelectingType = _setValue;

            SmartChildSelectingOption _getValue;
            _getValue = _target.SmartChildSelectingType;

            Assert.AreEqual(_setValue, _getValue);

        }

        [Test]
        public void VerifySmartImageSelectSupport()
        {
            _treeView.ImageList = new ImageList();

            bool _getValue;

            _getValue = _target.SmartImageSelectSupport;

            bool _setValue = !_getValue;

            _target.SmartImageSelectSupport = _setValue;
            _getValue = _target.SmartImageSelectSupport;

            Assert.AreEqual(_setValue, _getValue);
        }

        [Test]
        public void VerifySmartCheckBoxSelectSupport()
        {
            bool _getValue;

            _getValue = _target.SmartCheckBoxSelectSupport;

            bool _setValue = !_getValue;

            _treeView.CheckBoxes = true;
            _target.SmartCheckBoxSelectSupport = _setValue;
            _getValue = _target.SmartCheckBoxSelectSupport;

            Assert.AreEqual(_setValue, _getValue);
        }

        #endregion

        #region Method Tests

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        public void VerifySmartCheckBoxSelectSupportWorksOnlyIfSwitchedOn()
        {
            _treeView.CheckBoxes = true;
            _treeView.Nodes.Add("Root");
            _treeView.Nodes[0].Nodes.Add("A");
            _treeView.Nodes[0].Nodes[0].Nodes.Add("A1");
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1a");
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1b");

            _target.SmartCheckBoxSelectSupport = false;
            _treeView.Nodes[0].Nodes[0].Checked = true;
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Nodes[0].Checked);

            _treeView.Nodes[0].Nodes[0].Checked = false;
            _target.SmartCheckBoxSelectSupport = true;
            _treeView.Nodes[0].Nodes[0].Checked = true;
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Checked);
        }

        /// <summary>
        /// Verifies the branch check works.
        /// </summary>
        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        public void VerifySmartCheckBoxSelectSupportWorks()
        {
            _treeView.CheckBoxes = true;
            _target.SmartCheckBoxSelectSupport = true;
            _treeView.Nodes.Add("Root");
            _treeView.Nodes[0].Nodes.Add("A");
            _treeView.Nodes[0].Nodes.Add("B");
            _treeView.Nodes[0].Nodes[0].Nodes.Add("A1");
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1a");
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A1b");
            _treeView.Nodes[0].Nodes[0].Nodes.Add("A2");
            _treeView.Nodes[0].Nodes[1].Nodes.Add("B1");
            _treeView.Nodes[0].Nodes[1].Nodes.Add("B2");
            _treeView.Nodes[0].Nodes[1].Nodes[0].Nodes.Add("B1a");
            _treeView.Nodes[0].Nodes[1].Nodes[0].Nodes.Add("B1b");
            
            _treeView.Nodes[0].Nodes[0].Checked = true;
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Checked);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[1].Checked);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Checked);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].Nodes[0].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].Nodes[1].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[0].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[1].Nodes[1].Checked);

            _treeView.Nodes[0].Nodes[0].Checked = false;
            _treeView.Nodes[0].Nodes[1].Checked = true;
            Assert.IsTrue(_treeView.Nodes[0].Nodes[1].Nodes[0].Checked);
            Assert.IsTrue(_treeView.Nodes[0].Nodes[1].Nodes[1].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Nodes[0].Checked);
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Nodes[1].Checked);

            _target.SmartParentSelectingType = SmartParentSelectingOption.SelectParentIfAllChildSelected;
            _treeView.Nodes[0].Nodes[0].Checked = true;
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked = false;
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Checked, "If a child is unchecked, the parent should be unchecked also.");
            Assert.IsFalse(_treeView.Nodes[0].Checked);

            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked = true;
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Checked, "If all children are checked, the parent should be checked also.");

            _target.SmartParentSelectingType = SmartParentSelectingOption.SelectParentIfAtLeastOneChildSelected;
            _treeView.Nodes[0].Nodes[0].Checked = true;
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[1].Checked = false;
            Assert.IsTrue(_treeView.Nodes[0].Nodes[0].Checked, "Only if all children are unchecked, the parent should be unchecked also.");
            Assert.IsTrue(_treeView.Nodes[0].Checked);
            _treeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Checked = false;
            _treeView.Nodes[0].Nodes[0].Nodes[1].Checked = false;
            Assert.IsFalse(_treeView.Nodes[0].Nodes[0].Checked, "If all children are unchecked, the parent should be unchecked also.");

        }

        /// <summary>
        /// Verifies that the branch check exception thrown if check boxes not set
        /// vhen the personItem want to use branch check property.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifySmartCheckBoxSelectSupportExceptionIfCheckBoxesNotSet()
        { 
            _treeView.CheckBoxes = false;
            _target.SmartCheckBoxSelectSupport = true;
        
        }

        /// <summary>
        /// Verifies that the branch check exception thrown if check boxes not set
        /// vhen the personItem want to use branch check property.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifySmartCheckBoxSelectSupportExceptionIfSmartImageSelectSupportIsOn()
        {
            _treeView.CheckBoxes = true;
            _target.SmartImageSelectSupport = true;
            _target.SmartCheckBoxSelectSupport = true;

        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifySmartImageSelectSupportExceptionIfSmartCheckBoxSelectSupportIsOn()
        {
            _treeView.CheckBoxes = true;
            _target.SmartCheckBoxSelectSupport = true;
            _target.SmartImageSelectSupport = true;

        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifySmartImageSelectSupportExceptionIfImageListIsNotSet()
        {
            _treeView.ImageList = null;
            _target.SmartCheckBoxSelectSupport = false;
            _target.SmartImageSelectSupport = true;

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