using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Collections;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    /// <summary>
    /// Test methods for TreeItem class
    /// </summary>
    [TestFixture]
    public class TreeItemTest
    {

        #region Variables

        private TreeItem<TreeNode> _treeRoot;
        private TreeItem<TreeNode> _treeItem;

        #endregion

        #region Test Preparation Methods

        /// <summary>
        /// Initialise tests.
        /// </summary>
        [SetUp]
        public void TestInit()
        {
            _treeRoot = new TreeItem<TreeNode>("Root");
            _treeItem = new TreeItem<TreeNode>(_treeRoot, null, "Key", "Text", "Tag", null);
        }

        #endregion

        #region Constructor tests

        /// <summary>
        /// Constructor test.
        /// </summary>
        [Test]
        public void VerifyCreateTreeItem()
        {
            // Note: initialisation is performed in TestInit just check result here

            // Declare return type to hold constructor result

            TreeItem<TreeNode> returnValue = _treeItem;
            Assert.IsNotNull(returnValue);

            returnValue = _treeRoot;
            Assert.IsNotNull(returnValue);

            // Check Parent
            Assert.AreSame(_treeItem.Parent, _treeRoot);
        }

        [Test]
        public void VerifyCreateTreeItemOverload1()
        {
            // Declare variables to pass to constructor call
            TreeNode node = null;
            const string key = "Key1";
            const string text = "Test1";
            const string tag = "Tag1";
            object data = null;

            // Declare return type to hold constructor result

            // Instantiate object
            var returnValue = new TreeItem<TreeNode>(_treeRoot, node, key, text, tag, data);

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

            // Check Parent
            Assert.AreSame(returnValue.Parent, _treeRoot);
            Assert.AreSame(returnValue.Node, node);

        }

        [Test]
        public void VerifyCreateTreeItemOverload2()
        {
            // Declare variables to pass to constructor call
            const string key = "Key2";
            const string text = "Test2";

            // Declare return type to hold constructor result
            var returnValue = new TreeItem<TreeNode>(_treeRoot, key, text);

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

            // Check Parent
            Assert.AreSame(returnValue.Parent, _treeRoot);
        }

        [Test]
        public void VerifyCreateTreeItemOverload3()
        {
            // Declare variables to pass to constructor call
            const string text = "Text3";

            // Declare return type to hold constructor result

            // Instantiate object
            var returnValue = new TreeItem<TreeNode>(_treeRoot, text);

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

            // Check Parent
            Assert.AreSame(returnValue.Parent, _treeRoot);

        }

        [Test]
        public void VerifyCreateTreeItemOverload4()
        {
            // Declare variables to pass to constructor call
            const string key4 = "key4";
            const string text4 = "text4";
            const string tag4 = "tag4";
            var object4 = new object();

            // Declare return type to hold constructor result

            // Instantiate object
            var returnValue = new TreeItem<TreeNode>(_treeRoot, key4, text4, tag4, object4);

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

            // Check Parent
            Assert.AreSame(returnValue.Parent, _treeRoot);

        }

        #endregion


        #region Method Tests

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEnumerate()
        {
            /*
            - ROOT
                |_ITEM1
                |   |_ITEM11
                |   |_ITEM12
                |_ITEM2  
                    |_ITEM21

            */
            ITreeItem<TreeNode> root = new TreeItem<TreeNode>("ROOT");
            ITreeItem<TreeNode> item1 = new TreeItem<TreeNode>(root, "ITEM1");
            ITreeItem<TreeNode> item2 = new TreeItem<TreeNode>(root, "ITEM2");
            ITreeItem<TreeNode> item11 = new TreeItem<TreeNode>(item1, "ITEM11");
            ITreeItem<TreeNode> item12 = new TreeItem<TreeNode>(item1, "ITEM12");
            ITreeItem<TreeNode> item21 = new TreeItem<TreeNode>(item2, "ITEM21");

            IEnumerable<ITreeItem<TreeNode>> result = root.Enumerate(RangeOption.ThisOnly);
            IList<ITreeItem<TreeNode>> resultList = new List<ITreeItem<TreeNode>>(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreSame(root, resultList[0]);

            result = root.Enumerate(RangeOption.Children);
            resultList = new List<ITreeItem<TreeNode>>(result);
            Assert.AreEqual(2, resultList.Count);
            Assert.AreSame(item1, resultList[0]);
            Assert.AreSame(item2, resultList[1]);

            result = root.Enumerate(RangeOption.ThisAndChildren);
            resultList = new List<ITreeItem<TreeNode>>(result);
            Assert.AreEqual(3, resultList.Count);
            Assert.AreSame(root, resultList[0]);
            Assert.AreSame(item1, resultList[1]);
            Assert.AreSame(item2, resultList[2]);

            result = root.Enumerate(RangeOption.RecursiveChildren);
            resultList = new List<ITreeItem<TreeNode>>(result);
            Assert.AreEqual(5, resultList.Count);
            Assert.AreSame(item1, resultList[0]);
            Assert.AreSame(item11, resultList[1]);
            Assert.AreSame(item12, resultList[2]);
            Assert.AreSame(item2, resultList[3]);
            Assert.AreSame(item21, resultList[4]);

            result = root.Enumerate(RangeOption.ThisAndRecursiveChildren);
            resultList = new List<ITreeItem<TreeNode>>(result);
            Assert.AreEqual(6, resultList.Count);
            Assert.AreSame(root, resultList[0]);
            Assert.AreSame(item1, resultList[1]);
            Assert.AreSame(item11, resultList[2]);
            Assert.AreSame(item12, resultList[3]);
            Assert.AreSame(item2, resultList[4]);
            Assert.AreSame(item21, resultList[5]);

        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItem()
        {
            // Declare variables to pass to method call
            const string key = "Key";

            // Declare return type to hold method result
            ITreeItem<TreeNode> expectedValue = _treeItem;

            ITreeItem<TreeNode> returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItem1()
        {
            // Declare variables to pass to method call
            const string text = "Text1";

            // Make method call
            var expectedValue = _treeItem.AddChild(text);

            var returnValue = _treeItem.FindItem(text, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload2()
        {
            // Declare variables to pass to method call
            const string text = "Text2";
            const string key = "Key2";

            var expectedValue = _treeItem.AddChild(key, text);

            var returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            var isRemoved = _treeItem.RemoveChild(key, false);

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(key, RangeOption.ThisAndChildren));
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload3()
        {
            var originalCount = _treeItem.ChildCollection.Count;

            // Make method call with null value
            ITreeItem<TreeNode> returnValue = null;
            returnValue = _treeItem.AddChild(returnValue);

            Assert.IsNull(returnValue);
            Assert.AreEqual(originalCount, _treeItem.ChildCollection.Count);

            const string key = "Key3";

            // Declare return type to hold method result
            ITreeItem<TreeNode> expectedValue = new TreeItem<TreeNode>(key);

            // Make method call
            expectedValue = _treeItem.AddChild(expectedValue);

            returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            // not to remove if not direct child
            var isRemoved = _treeRoot.RemoveChild(key, false);
            Assert.IsFalse(isRemoved);

             isRemoved = _treeItem.RemoveChild(returnValue);

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(key, RangeOption.ThisAndChildren));
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload4()
        {
            // Declare variables to pass to constructor call
            TreeNode node = null;
            const string key41 = "Key41";
            const string text41 = "Text41";

            // Make method call and perform Assert Tests
            var expectedValue = _treeItem.AddChild(node, key41, text41);

            var returnValue = _treeItem.FindItem(key41, RangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(key41, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key41, RangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            const string key42 = "Key42";
            const string text42 = "Text42";

            expectedValue = _treeItem.ChildCollection[0].AddChild(key42, text42);
            returnValue = _treeRoot.FindItem(key42, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key42, RangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            bool remove = _treeRoot.RemoveChild(key42, false);
            Assert.IsFalse(remove);

            remove = _treeRoot.RemoveChild(returnValue);
            Assert.IsFalse(remove);

            remove = _treeRoot.RemoveChild(key42, true);
            Assert.IsTrue(remove);

        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload5()
        {
            // Declare variables to pass to method call
            var node = new TreeNode();
            TreeNode nullNode = null;
            const string key = "Key5";
            const string text = "Text5";
            const string tag = "Tag5";
            object data = new TreeNode();

            // Verify findItem with null input
            var returnValue = _treeItem.FindItem(nullNode, RangeOption.ThisAndRecursiveChildren);
            Assert.IsNull(returnValue);

            ITreeItem<TreeNode> expectedValue = _treeItem.AddChild(node, key, text, tag, data);

            // Perform Assert Tests
            returnValue = _treeItem.FindItem(node, RangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(node, RangeOption.ThisAndRecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            var isRemoved = _treeRoot.RemoveChild(node, false);
            Assert.IsFalse(isRemoved);

            isRemoved = _treeItem.RemoveChild(node, false);

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(node, RangeOption.ThisAndChildren));
            Assert.IsNull(_treeItem.FindItem(node, RangeOption.ThisAndRecursiveChildren));
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload6()
        {
            // Declare variables to pass to method call
            const string key = "Key6";
            const string text = "Text6";
            const string tag = "Tag6";
            var data = new TreeNode();

            // Make method call
            var expectedValue = _treeItem.AddChild(key, text, tag, data);
            var returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyFindItemByData()
        {
            // Declare variables to pass to method call
            const string key = "KeyData";
            const string text = "TextData";
            const string tag = "TagData";
            var data = new object();

            // Make method call
            var expectedValue = _treeItem.AddChild(key, text, tag, data);
            var returnValue = _treeItem.FindItem(data, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(data, RangeOption.ThisAndRecursiveChildren);
            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(data, RangeOption.ThisAndChildren);
            // Perform Assert Tests
            Assert.IsNull(returnValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyRecursiveSelectRangeProperty()
        {
            /*
            - ROOT
                |_ITEM1
                |   |_ITEM11
                |   |_ITEM12
                |_ITEM2  
                    |_ITEM21

            */

            ITreeItem<TreeNode> root = new TreeItem<TreeNode>("ROOT");
            ITreeItem<TreeNode> item1 = new TreeItem<TreeNode>(root, "ITEM1");
            ITreeItem<TreeNode> item2 = new TreeItem<TreeNode>(root, "ITEM2");
            ITreeItem<TreeNode> item11 = new TreeItem<TreeNode>(item1, "ITEM11");
            ITreeItem<TreeNode> item12 = new TreeItem<TreeNode>(item1, "ITEM12");
            ITreeItem<TreeNode> item21 = new TreeItem<TreeNode>(item2, "ITEM21");

            item1.AutoChildSelection = true;
            Assert.IsFalse(root.AutoChildSelection);
            Assert.IsTrue(item1.AutoChildSelection);
            Assert.IsFalse(item2.AutoChildSelection);
            Assert.IsTrue(item11.AutoChildSelection);
            Assert.IsTrue(item12.AutoChildSelection);
            Assert.IsFalse(item21.AutoChildSelection);

            //// note: the higher hierarchy will override the seetings of the children.
            root.AutoChildSelection = true;
            Assert.IsTrue(root.AutoChildSelection);
            Assert.IsTrue(item1.AutoChildSelection);
            Assert.IsTrue(item2.AutoChildSelection);
            Assert.IsTrue(item11.AutoChildSelection);
            Assert.IsTrue(item12.AutoChildSelection);
            Assert.IsTrue(item21.AutoChildSelection);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifySelectMethodRelatedProperties()
        {
            /*
            - ROOT
                |_ITEM1
                |   |_ITEM11
                |   |_ITEM12
                |_ITEM2  
                    |_ITEM21
                        |_ITEM211

            */

            ITreeItem<TreeNode> root = new TreeItem<TreeNode>("ROOT");
            ITreeItem<TreeNode> item1 = new TreeItem<TreeNode>(root, "ITEM1");
            ITreeItem<TreeNode> item2 = new TreeItem<TreeNode>(root, "ITEM2");
            ITreeItem<TreeNode> item11 = new TreeItem<TreeNode>(item1, "ITEM11");
            ITreeItem<TreeNode> item12 = new TreeItem<TreeNode>(item1, "ITEM12");
            ITreeItem<TreeNode> item21 = new TreeItem<TreeNode>(item2, "ITEM21");
            ITreeItem<TreeNode> item211 = new TreeItem<TreeNode>(item21, "ITEM211");

            /*
            - ROOT
                |_ITEM1
                |   |_ITEM11
                |   |_ITEM12
                |_*ITEM2  
                    |_*ITEM21
                        |_ITEM211

            */
            item2.AutoChildSelection = true;
            item21.AutoChildSelection = false;
            item2.Selected = true;

            Assert.IsTrue(item2.Selected);
            Assert.IsTrue(item21.Selected);
            Assert.IsFalse(item211.Selected);

            Assert.AreEqual(SelectedChildrenOption.SomeSelected, root.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item2.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item21.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item211.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item1.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item11.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item12.SelectedChildren);


            /*
            - *ROOT
                |_*ITEM1
                |   |_*ITEM11
                |   |_*ITEM12
                |_*ITEM2  
                    |_*ITEM21
                        |_*ITEM211

            */

            root.AutoChildSelection = true;
            root.Selected = true;

            Assert.IsTrue(root.Selected);
            Assert.IsTrue(item1.Selected);
            Assert.IsTrue(item11.Selected);
            Assert.IsTrue(item12.Selected);
            Assert.IsTrue(item2.Selected);
            Assert.IsTrue(item21.Selected);
            Assert.IsTrue(item211.Selected);

            Assert.AreEqual(SelectedChildrenOption.AllSelected, root.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item2.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item21.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item211.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item1.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item11.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item12.SelectedChildren);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifySmartParentSelectMethod()
        {
            /*
            - ROOT
                |_ITEM1
                |   |_ITEM11
                |   |_ITEM12
                |_*ITEM2  
                    |_*ITEM21
                        |_*ITEM211

            */

            ITreeItem<TreeNode> root = new TreeItem<TreeNode>("ROOT");
            ITreeItem<TreeNode> item1 = new TreeItem<TreeNode>(root, "ITEM1");
            ITreeItem<TreeNode> item2 = new TreeItem<TreeNode>(root, "ITEM2");
            ITreeItem<TreeNode> item11 = new TreeItem<TreeNode>(item1, "ITEM11");
            ITreeItem<TreeNode> item12 = new TreeItem<TreeNode>(item1, "ITEM12");
            ITreeItem<TreeNode> item21 = new TreeItem<TreeNode>(item2, "ITEM21");
            ITreeItem<TreeNode> item211 = new TreeItem<TreeNode>(item21, "ITEM211");


            root.AutoChildSelection = true;
            item2.Selected = true;

            Assert.IsTrue(item2.Selected);
            Assert.IsTrue(item21.Selected);
            Assert.IsTrue(item211.Selected);

            Assert.AreEqual(SelectedChildrenOption.SomeSelected, root.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item2.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item21.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item211.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item1.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item11.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item12.SelectedChildren);

            item1.AutoChildSelection = true;
            root.AutoParentSelection = AutoParentSelectionOption.SelectParentIfAllChildSelected;
            item1.Selected = true;
            // must become like that

            /*
            - *ROOT
                |_*ITEM1
                |   |_*ITEM11
                |   |_*ITEM12
                |_*ITEM2  
                    |_*ITEM21
                        |_*ITEM211

            */

            Assert.IsTrue(root.Selected);
            Assert.IsTrue(item1.Selected);
            Assert.IsTrue(item11.Selected);
            Assert.IsTrue(item12.Selected);
            Assert.IsTrue(item2.Selected);
            Assert.IsTrue(item21.Selected);
            Assert.IsTrue(item211.Selected);

            Assert.AreEqual(SelectedChildrenOption.AllSelected, root.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item2.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item21.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item211.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.AllSelected, item1.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item11.SelectedChildren);
            Assert.AreEqual(SelectedChildrenOption.NoneSelected, item12.SelectedChildren);

        }

        [Test]
        public void VerifyEvents()
        {
            // Initialise with null
            const string key = "Key3";

            _treeItem.Adding += TreeItemAdding;
            _treeItem.Added += TreeItemAdded;

            // Declare return type to hold method result
            var expectedValue = new TreeItem<TreeNode>(key);

            // Make method call
            var returnValue = _treeItem.AddChild(expectedValue);

            Assert.IsNull(returnValue);

            returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            Assert.IsTrue(_addingEventCalled);
            Assert.IsFalse(_addedEventCalled);

            _addingEventCalled = false;

            // Make method call
            returnValue = _treeItem.InsertChild(0, expectedValue);

            Assert.IsNull(returnValue);

            returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            Assert.IsTrue(_addingEventCalled);
            Assert.IsFalse(_addedEventCalled);

            _addingEventCalled = false;

            _treeItem.Adding -= TreeItemAdding;

            // Make method call again
            returnValue = _treeItem.AddChild(expectedValue);

            // Asser result
            Assert.AreSame(expectedValue, returnValue);
            Assert.IsTrue(_addedEventCalled);

            _treeItem.Removing += TreeItemRemoving;
            _treeItem.Removed += TreeItemRemoved;

            var removeResult = _treeItem.RemoveChild(key, false);

            Assert.IsFalse(removeResult);
            Assert.IsTrue(_removingEventCalled);
            Assert.IsFalse(_removedEventCalled);

            _treeItem.Removing -= TreeItemRemoving;

            removeResult = _treeItem.RemoveChild(key, false);

            Assert.IsTrue(removeResult);
            Assert.IsTrue(_removedEventCalled);

        }

        private bool _removedEventCalled;

        void TreeItemRemoved(object sender, TreeItemEventArgs<TreeNode> e)
        {
            _removedEventCalled = true;
        }

        private bool _removingEventCalled;

        void TreeItemRemoving(object sender, TreeItemEventArgs<TreeNode> e)
        {
            _removingEventCalled = true;
            e.Cancel = true;
        }

        private bool _addingEventCalled;

        void TreeItemAdding(object sender, TreeItemEventArgs<TreeNode> e)
        {
            _addingEventCalled = true;
            e.Cancel = true;
        }

        private bool _addedEventCalled;

        void TreeItemAdded(object sender, TreeItemEventArgs<TreeNode> e)
        {
            _addedEventCalled = true;
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildRemoveChild()
        {
            // Declare variables to pass to method call
            const string key = "Key1";

            // Declare return type to hold method result
            ITreeItem<TreeNode> expectedValue = null;
            const int position = 0;

            // verify null input
            var returnValue = _treeRoot.InsertChild(position, expectedValue);
            Assert.IsNull(returnValue);

            // Verify remove null
            var removed = _treeItem.RemoveChild(expectedValue);
            Assert.IsFalse(removed);

            expectedValue = new TreeItem<TreeNode>(key);
            expectedValue = _treeRoot.InsertChild(position, expectedValue);

            // Verify if added

            returnValue = _treeRoot.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            // Verify position
            returnValue = _treeRoot.ChildCollection[position];
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildOverload2()
        {
            // Declare variables to pass to method call
            const string key = "Key2";

            // Declare return type to hold method result
            const int position = 0;

            var expectedValue = _treeRoot.InsertChild(position, key);

            // Verify if added
            ITreeItem<TreeNode> returnValue = _treeRoot.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            // Verify position
            returnValue = _treeRoot.ChildCollection[position];
            Assert.AreSame(expectedValue, returnValue);
        }

        [Test]
        public void VerifyInsertChildOverload3()
        {
            // Declare variables to pass to method call
            const string key = "Key3";
            const string text = "Text3";

            // Declare return type to hold method result
            const int position = 0;

            var expectedValue = _treeRoot.InsertChild(position, key, text);

            // Verify if added
            var returnValue = _treeRoot.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            // Verify position
            returnValue = _treeRoot.ChildCollection[position];
            Assert.AreSame(expectedValue, returnValue);

        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildOverload4()
        {
            // Declare variables to pass to constructor call
            TreeNode node = null;
            const string key = "Key4";
            const string text = "Text4";
            const int position = 0;

            // Make method call
            var expectedValue = _treeItem.InsertChild(position, node, key, text);
            var returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(key, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key, RangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildOverload5()
        {
            // Declare variables to pass to method call
            var node = new TreeNode();
            const string key = "Key5";
            const string text = "Text5";
            const string tag = "Tag5";
            var data = new TreeNode();
            const int position = 0;

            // Make method call
            var expectedValue = _treeItem.InsertChild(position, node, key, text, tag, data);

            // Perform Assert Tests
            var returnValue = _treeItem.FindItem(node, RangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(node, RangeOption.ThisAndRecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            var isRemoved = _treeItem.RemoveChild(node, false);

            Assert.IsTrue(isRemoved);

            returnValue = _treeItem.FindItem(node, RangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);
            returnValue = _treeItem.FindItem(node, RangeOption.ThisAndRecursiveChildren);
            Assert.IsNull(returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildOverload6()
        {
            // Declare variables to pass to method call
            const string key = "Key6";
            const string text = "Text6";
            const string tag = "Tag6";
            var data = new TreeNode();
            const int position = 0;

             // Make method call
            var expectedValue = _treeItem.InsertChild(position, key, text, tag, data);
            var returnValue = _treeItem.FindItem(key, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// Remove test case
        /// </summary>
        [Test]
        public void VerifyRemove()
        {
            // Declare variables to pass to method call
            const string text = "Text1";

            // Remove with no Parent should return false
            Assert.IsFalse(_treeRoot.Remove());

            ITreeItem<TreeNode> expectedValue = new TreeItem<TreeNode>(text);
            expectedValue = _treeItem.AddChild(expectedValue);

            // Make method call
            var returnValue = _treeItem.FindItem(text, RangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            bool isRemoved = expectedValue.Remove();

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(expectedValue, RangeOption.ThisAndChildren));
        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Test the Key property.
        /// </summary>
        [Test]
        public void VerifyKey()
        {
            // Test get method
            var getValue = _treeItem.Key;

            // Perform Assert Tests
            Assert.AreEqual(getValue, "Key");
        }

        /// <summary>
        /// Test the IsLeaf property.
        /// </summary>
        [Test]
        public void VerifyIsLeaf()
        {
            // Test get method
            var getValue = _treeItem.IsLeaf;

            Assert.IsFalse(getValue);

            // Declare variable to hold property set method
            const bool setValue = true;

            // Test set method
            _treeItem.IsLeaf = setValue;

            // Test get method
            getValue = _treeItem.IsLeaf;

            // Perform Assert Tests
            Assert.IsTrue(getValue);
        }

        /// <summary>
        /// Test the Key property.
        /// </summary>
        [Test]
        public void VerifyLevel()
        {
            var level2Item = _treeItem.AddChild("ChildToTreeItem"); 
            // Test get method
            var getValue = _treeItem.Level;
            Assert.AreEqual(getValue, 1);

            getValue = _treeRoot.Level;
            Assert.AreEqual(getValue, 0);

            getValue = level2Item.Level;
            Assert.AreEqual(getValue, 2);
        }

        /// <summary>
        /// Test the Text property.
        /// </summary>
        [Test]
        public void VerifyText()
        {
            // Declare return variable to hold property get method

            // Test set method
            const string setText = "set Text";
            _treeItem.Text = setText;

            // Test get method
            string getValue = _treeItem.Text;

            // Perform Assert Tests
            Assert.AreEqual(getValue, setText);
        }

        /// <summary>
        /// Test the ChildCollection property.
        /// </summary>
        [Test]
        public void VerifyChildren()
        {
            // Test get method
            var getValue = _treeRoot.ChildCollection;

            // Perform Assert Tests
            Assert.AreEqual(getValue.Count, 1);
        }

        /// <summary>
        /// Test the ResursiveList property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.WinCode.Common.Collections.TreeItem`1<System.Windows.Forms.TreeNode>"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item")]
        [Test]
        public void VerifyRecursiveChildCollection()
        {
            var item2 = new TreeItem<TreeNode>(_treeRoot, "Key2", "Text2");

            // Test get method
            var getValue = _treeRoot.RecursiveChildCollection;

            // Perform Assert Tests
            Assert.AreEqual(3, getValue.Count);

            new TreeItem<TreeNode>(_treeItem, "Key3", "Text3");
            var item4 = new TreeItem<TreeNode>(item2, "Key4", "Text4");
            new TreeItem<TreeNode>(item4, "Key5", "Text5");

            // Test get method
            getValue = _treeRoot.RecursiveChildCollection;
            // Perform Assert Tests
            Assert.AreEqual(6, getValue.Count);
        }

        [Test]
        public void VerifyRecursiveParentCollection()
        {
            // Test get method
            var getValue = _treeRoot.RecursiveParentCollection;

            // Perform Assert Tests
            Assert.AreEqual(0, getValue.Count);

            // Test get method
            getValue = _treeItem.RecursiveParentCollection;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
            Assert.AreSame(_treeRoot, getValue[0]);

            var item2 = new TreeItem<TreeNode>(_treeItem, "Key2", "Text2");
            var item3 = new TreeItem<TreeNode>(item2, "Key3", "Text3");
            var item4 = new TreeItem<TreeNode>(item3, "Key4", "Text4");
            var item5 = new TreeItem<TreeNode>(item4, "Key5", "Text5");

            // Test get method
            getValue = item5.RecursiveParentCollection;
            // Perform Assert Tests
            Assert.AreEqual(5, getValue.Count);
            Assert.AreSame(_treeRoot, getValue[0]);
            Assert.AreSame(_treeItem, getValue[1]);
            Assert.AreSame(item2, getValue[2]);
            Assert.AreSame(item3, getValue[3]);
            Assert.AreSame(item4, getValue[4]);
        }

        /// <summary>
        /// Test the Node property.
        /// </summary>
        [Test]
        public void VerifyNode()
        {
           // Declare variable to hold property set method
            var setValue = new TreeNode();

            // Test set method
            _treeItem.Node = setValue;

            // Test get method
            var getValue = _treeItem.Node;

            // Perform Assert Tests
            Assert.IsInstanceOf<TreeNode>(getValue);
        }

        /// <summary>
        /// Test the Tag property.
        /// </summary>
        [Test]
        public void VerifyTag()
        {
            // Declare variable to hold property set method
            const string setValue = "Tag3";

            // Test set method
            _treeItem.DescriptionText = setValue;

            // Test get method
            var getValue = _treeItem.DescriptionText;

            // Perform Assert Tests
            Assert.AreEqual(getValue, "Tag3");
        }

        /// <summary>
        /// Test the Data property.
        /// </summary>
        [Test]
        public void VerifyData()
        {
            // Declare variable to hold property set method
            var setValue = new TreeNode();

            // Test set method
            _treeItem.Data = setValue;

            // Test get method
            var getValue = _treeItem.Data;

            // Perform Assert Tests
            Assert.IsInstanceOf<TreeNode>(getValue);
        }

        /// <summary>
        /// Test the ImageIndex property.
        /// </summary>
        [Test]
        public void VerifyImageIndex()
        {
            // Declare variable to hold property set method
            var setValue = 2;

            // Test set method
            _treeItem.ImageIndex = setValue;

            // Test get method
            var getValue = _treeItem.ImageIndex;

            // Perform Assert Tests
            Assert.AreEqual(getValue, setValue);

            // set back to -1
            setValue = -1;

            _treeItem.ImageIndex = setValue;
            _treeRoot.ImageIndex = setValue;

            // add new 'not leaf' and 'leaf' items to _treeItem
            var leafItem = new TreeItem<TreeNode>("LeafItem") {IsLeaf = true};
            var notLeafItem = new TreeItem<TreeNode>("NotLeafItem") {IsLeaf = false};

            _treeItem.AddChild(leafItem);
            _treeItem.AddChild(notLeafItem);

            Assert.AreEqual(0, _treeRoot.ImageIndex);
            Assert.AreEqual(1, notLeafItem.ImageIndex);
            Assert.AreEqual(2, leafItem.ImageIndex);
        }

        /// <summary>
        /// Test the Parent property.
        /// </summary>
        [Test]
        public void VerifyParent()
        {
            // Declare return variable to hold property get method
            var oldChildValue = _treeRoot.ChildCollection.Count;

            // Declare return type to hold constructor result
            var newValue = new TreeItem<TreeNode>("Key2", "Text2") {Parent = _treeRoot};

            // Test set method

            // Test get method
            var getValue = newValue.Parent;

            // Perform Assert Tests
            Assert.AreSame(_treeRoot, getValue);

            // Has only this child once
            Assert.AreEqual(oldChildValue + 1, _treeRoot.ChildCollection.Count);
        }

        /// <summary>
        /// Test the HasChildren property.
        /// </summary>
        [Test]
        public void VerifyHasChildren()
        {
            // Test get method
            var getValue = _treeItem.HasChildren;

            Assert.IsFalse(getValue);

            // Perform Assert Tests
             new TreeItem<TreeNode>(_treeItem, "Key3", "Text3");

            // Test get method
            getValue = _treeItem.HasChildren;

            Assert.IsTrue(getValue);
        }

        [Test]
        public void VerifySelectedStatus()
        {
            Assert.IsFalse(_treeRoot.Selected);
            _treeRoot.Selected = true;
            Assert.IsTrue(_treeRoot.Selected);
            _treeRoot.Selected = false;
            Assert.IsFalse(_treeRoot.Selected);
        }

        /// <summary>
        /// Test the HasParent property.
        /// </summary>
        [Test]
        public void VerifyHasParent()
        {
            // Perform Assert Tests
            var newItem = new TreeItem<TreeNode>("Key3", "Text3");

            // Test get method
            var getValue = newItem.HasParent;

            Assert.IsFalse(getValue);

            newItem.Parent = _treeRoot;

            // Test get method
            getValue = newItem.HasParent;

            Assert.IsTrue(getValue);
        }

        /// <summary>
        /// Test the Data property.
        /// </summary>
        [Test]
        public void VerifyNodeDisplayInfo()
        {
            // Declare variable to hold property set method
            ITreeNodeDisplayInfo setValue = new TreeNodeDisplayInfo(new TreeNode());

            // Test set method
            _treeItem.NodeDisplayInfo = setValue;

            // Test get method
            object getValue = _treeItem.NodeDisplayInfo;

            // Perform Assert Tests
            Assert.AreSame(setValue, getValue);
        }

        [Test]
        public void VerifyIsPermitted()
        {
            const bool expected = false;
            _treeRoot.IsPermitted = expected;
            var result = _treeRoot.IsPermitted;
            Assert.AreEqual(expected, result);
        }

        #endregion
    }
}

