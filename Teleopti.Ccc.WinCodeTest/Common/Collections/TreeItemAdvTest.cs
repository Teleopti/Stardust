using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Collections;
using System.Windows.Forms;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Common.ControlBinders;
using Syncfusion.Windows.Forms.Tools;



namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    /// <summary>
    /// Test methods for TreeItem class
    /// </summary>
    [TestFixture]
    public class TreeItemAdvTest
    {

        #region Variables

        private TreeItem<TreeNodeAdv> _treeRoot;
        private TreeItem<TreeNodeAdv> _treeItem;

        #endregion

        #region Test Preparation Methods


        //[TestFixtureSetUp]
        //public void FixtureInit()
        //{
        //}

        //[TestFixtureTearDown]
        //public void FixtureDispose()
        //{

        //}

        /// <summary>
        /// Initialise tests.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.Object,System.String,System.String,System.String,System.Object)")]
        [SetUp]
        public void TestInit()
        {
            _treeRoot = new TreeItem<TreeNodeAdv>("Root");
            _treeItem = new TreeItem<TreeNodeAdv>(_treeRoot, null, "Key", "Text", "Tag", null);
        }

        //[TearDown]
        //public void TestDispose()
        //{

        //}

        #endregion

        #region Constructor tests

        /// <summary>
        /// Constructor test.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.Object,System.String,System.String)")]
        [Test]
        public void VerifyCreateTreeItem()
        {
            // Note: initialisation is performed in TestInit just check result here

            // Declare return type to hold constructor result
            TreeItem<TreeNodeAdv> _returnValue;

            _returnValue = _treeItem;
            Assert.IsNotNull(_returnValue);

            _returnValue = _treeRoot;
            Assert.IsNotNull(_returnValue);

            // Check Parent
            Assert.AreSame(_treeItem.Parent, _treeRoot);
        }

        /// <summary>
        /// Constructor test.
        /// </summa
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.Object,System.String,System.String,System.String,System.Object)"), Test]
        public void VerifyCreateTreeItemOverload1()
        {
            // Declare variables to pass to constructor call
            TreeNodeAdv _node = null;
            string _key = "Key1";
            string _text = "Test1";
            string _tag = "Tag1";
            object _data = null;

            // Declare return type to hold constructor result
            TreeItem<TreeNodeAdv> _returnValue;

            // Instantiate object
            _returnValue = new TreeItem<TreeNodeAdv>(_treeRoot, _node, _key, _text, _tag, _data);

            // Perform Assert Tests
            Assert.IsNotNull(_returnValue);

            // Check Parent
            Assert.AreSame(_returnValue.Parent, _treeRoot);

        }

        /// <summary>
        /// Constructor test.
        /// </summa
        [Test]
        public void VerifyCreateTreeItemOverload2()
        {
            // Declare variables to pass to constructor call
            string _key = "Key2";
            string _text = "Test2";

            // Declare return type to hold constructor result
            TreeItem<TreeNodeAdv> _returnValue = new TreeItem<TreeNodeAdv>(_treeRoot, _key, _text);

            // Perform Assert Tests
            Assert.IsNotNull(_returnValue);

            // Check Parent
            Assert.AreSame(_returnValue.Parent, _treeRoot);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String)"), Test]
        public void VerifyCreateTreeItemOverload3()
        {
            // Declare variables to pass to constructor call
            string _text = "Text3";

            // Declare return type to hold constructor result
            TreeItem<TreeNodeAdv> _returnValue;

            // Instantiate object
            _returnValue = new TreeItem<TreeNodeAdv>(_treeRoot, _text);

            // Perform Assert Tests
            Assert.IsNotNull(_returnValue);

            // Check Parent
            Assert.AreSame(_returnValue.Parent, _treeRoot);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String)"), Test]
        public void VerifyCreateTreeItemOverload4()
        {
            // Declare variables to pass to constructor call
            string key4 = "key4";
            string text4 = "text4";
            string tag4 = "tag4";
            object object4 = new object();

            // Declare return type to hold constructor result
            ITreeItem<TreeNodeAdv> _returnValue;

            // Instantiate object
            _returnValue = new TreeItem<TreeNodeAdv>(_treeRoot, key4, text4, tag4, object4);

            // Perform Assert Tests
            Assert.IsNotNull(_returnValue);

            // Check Parent
            Assert.AreSame(_returnValue.Parent, _treeRoot);

        }

        #endregion

        #region Static Method Tests

        #endregion

        #region Method Tests

        //[Test]
        //public void VerifyJoinTreeItems()
        //{
        //    ITreeItem master = _treeItem;
        //    _treeRoot.JoinWith(master);
    
        //    // test add

        //    // test delete

        //    // test update >> properties

        //    // test udpate >> moving item to another parent

        //}

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItem()
        {
            // Declare variables to pass to method call
            string key = "Key";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            expectedValue = _treeItem;

            ITreeItem<TreeNodeAdv> returnValue;
            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItem1()
        {
            // Declare variables to pass to method call
            string text = "Text1";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;

            // Make method call
            expectedValue = _treeItem.AddChild(text);

            ITreeItem<TreeNodeAdv> returnValue;
            returnValue = _treeItem.FindItem(text, SearchRangeOption.ThisAndChildren);

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
            string text = "Text2";
            string key = "Key2";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            ITreeItem<TreeNodeAdv> returnValue;

            // Make method call
            expectedValue = _treeItem.AddChild(key, text);

            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            bool _isRemoved = _treeItem.RemoveChild(key, false);

            Assert.IsTrue(_isRemoved);
            Assert.IsNull(_treeItem.FindItem(key, SearchRangeOption.ThisAndChildren));
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload3()
        {

            // Initialise with null
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue;
            int _originalCount = _treeItem.ChildCollection.Count;

            // Make method call with null value
            returnValue = null;
            returnValue = _treeItem.AddChild(returnValue);

            Assert.IsNull(returnValue);
            Assert.AreEqual(_originalCount, _treeItem.ChildCollection.Count);

            string key = "Key3";

            // Declare return type to hold method result
            expectedValue = new TreeItem<TreeNodeAdv>(key);

            // Make method call
            expectedValue = _treeItem.AddChild(expectedValue);

            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            // not to remove if not direct child
            bool _isRemoved = _treeRoot.RemoveChild(key, false);
            Assert.IsFalse(_isRemoved);

             _isRemoved = _treeItem.RemoveChild(returnValue);

            Assert.IsTrue(_isRemoved);
            Assert.IsNull(_treeItem.FindItem(key, SearchRangeOption.ThisAndChildren));

            

        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload4()
        {
            // Declare variables to pass to constructor call
            TreeNodeAdv node = null;
            string key41 = "Key41";
            string text41 = "Text41";

            // Declare return type to hold constructor result
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue;
            bool remove;

            // Make method call and perform Assert Tests
            expectedValue = _treeItem.AddChild(node, key41, text41);

            returnValue = _treeItem.FindItem(key41, SearchRangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(key41, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key41, SearchRangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            string key42 = "Key42";
            string text42 = "Text42";

            expectedValue = _treeItem.ChildCollection[0].AddChild(key42, text42);
            returnValue = _treeRoot.FindItem(key42, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key42, SearchRangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            remove = _treeRoot.RemoveChild(key42, false);
            Assert.IsFalse(remove);

            remove = _treeRoot.RemoveChild(returnValue);
            Assert.IsFalse(remove);

            remove = _treeRoot.RemoveChild(key42, true);
            Assert.IsTrue(remove);

        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem.AddChild(System.Object,System.String,System.String,System.String,System.Object)")]
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload5()
        {
            // Declare variables to pass to method call
            TreeNodeAdv node = new TreeNodeAdv();
            TreeNodeAdv nullNode = null;
            string key = "Key5";
            string text = "Text5";
            string tag = "Tag5";
            object data = new TreeNodeAdv();

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue = null;

            // Verify findItem with null input
            returnValue = _treeItem.FindItem(expectedValue, SearchRangeOption.ThisAndRecursiveChildren);
            Assert.IsNull(returnValue);
            returnValue = _treeItem.FindItem(nullNode, SearchRangeOption.ThisAndRecursiveChildren);
            Assert.IsNull(returnValue);

            // Make method call
            expectedValue = _treeItem.AddChild(node, key, text, tag, data);

            // Perform Assert Tests
            returnValue = _treeItem.FindItem(node, SearchRangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(node, SearchRangeOption.ThisAndRecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            bool isRemoved = _treeRoot.RemoveChild(node, false);
            Assert.IsFalse(isRemoved);

            isRemoved = _treeItem.RemoveChild(node, false);

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(node, SearchRangeOption.ThisAndChildren));
            Assert.IsNull(_treeItem.FindItem(node, SearchRangeOption.ThisAndRecursiveChildren));
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem.AddChild(System.String,System.String,System.String,System.Object)")]
        [Test]
        public void VerifyAddChildRemoveChildFindChildFindItemOverload6()
        {
            // Declare variables to pass to method call
            //TODO: Set values for variables
            string key = "Key6";
            string text = "Text6";
            string tag = "Tag6";
            object data = new TreeNode();

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            ITreeItem<TreeNodeAdv> returnValue;

            // Make method call
            expectedValue = _treeItem.AddChild(key, text, tag, data);
            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem.AddChild(System.String,System.String,System.String,System.Object)")]
        [Test]
        public void VerifyFindItemByData()
        {
            // Declare variables to pass to method call
            //TODO: Set values for variables
            string key = "KeyData";
            string text = "TextData";
            string tag = "TagData";
            object data = new object();

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            ITreeItem<TreeNodeAdv> returnValue;

            // Make method call
            expectedValue = _treeItem.AddChild(key, text, tag, data);
            returnValue = _treeItem.FindItem(data, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(data, SearchRangeOption.ThisAndRecursiveChildren);
            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(data, SearchRangeOption.ThisAndChildren);
            // Perform Assert Tests
            Assert.IsNull(returnValue);


        }

        [Test]
        public void VerifyEvents()
        {

            // Initialise with null
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue;
            string key = "Key3";
            bool removeResult;

            _treeItem.Adding += new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Adding);
            _treeItem.Added += new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Added);

            // Declare return type to hold method result
            expectedValue = new TreeItem<TreeNodeAdv>(key);

            // Make method call
            returnValue = _treeItem.AddChild(expectedValue);

            Assert.IsNull(returnValue);

            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            Assert.IsTrue(_addingEventCalled);
            Assert.IsFalse(_addedEventCalled);

            _addingEventCalled = false;

            // Make method call
            returnValue = _treeItem.InsertChild(0, expectedValue);

            Assert.IsNull(returnValue);

            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            Assert.IsTrue(_addingEventCalled);
            Assert.IsFalse(_addedEventCalled);

            _addingEventCalled = false;

            _treeItem.Adding -= new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Adding);

            // Make method call again
            returnValue = _treeItem.AddChild(expectedValue);

            // Asser result
            Assert.AreSame(expectedValue, returnValue);
            Assert.IsTrue(_addedEventCalled);

            _treeItem.Removing += new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Removing);
            _treeItem.Removed += new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Removed);

            removeResult = _treeItem.RemoveChild(key, false);

            Assert.IsFalse(removeResult);
            Assert.IsTrue(_removingEventCalled);
            Assert.IsFalse(_removedEventCalled);

            _treeItem.Removing -= new EventHandler<TreeItemEventArgs<TreeNodeAdv>>(_treeItem_Removing);

            removeResult = _treeItem.RemoveChild(key, false);

            Assert.IsTrue(removeResult);
            Assert.IsTrue(_removedEventCalled);

        }

        private bool _removedEventCalled;

        void _treeItem_Removed(object sender, TreeItemEventArgs<TreeNodeAdv> e)
        {
            _removedEventCalled = true;
        }

        private bool _removingEventCalled;

        void _treeItem_Removing(object sender, TreeItemEventArgs<TreeNodeAdv> e)
        {
            _removingEventCalled = true;
            e.Cancel = true;
        }

        private bool _addingEventCalled;

        void _treeItem_Adding(object sender, TreeItemEventArgs<TreeNodeAdv> e)
        {
            _addingEventCalled = true;
            e.Cancel = true;
        }

        private bool _addedEventCalled;

        void _treeItem_Added(object sender, TreeItemEventArgs<TreeNodeAdv> e)
        {
            _addedEventCalled = true;
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [Test]
        public void VerifyInsertChildRemoveChild()
        {
            // Declare return type to hold method result
            bool removed;

            // Declare variables to pass to method call
            string key = "Key1";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue = null;
            ITreeItem<TreeNodeAdv> returnValue;
            int position = 0;

            // verify null input
            returnValue = _treeRoot.InsertChild(position, expectedValue);
            Assert.IsNull(returnValue);

            // Verify remove null
            removed = _treeItem.RemoveChild(expectedValue);
            Assert.IsFalse(removed);

            expectedValue = new TreeItem<TreeNodeAdv>(key);
            expectedValue = _treeRoot.InsertChild(position, expectedValue);

            // Verify if added

            returnValue = _treeRoot.FindItem(key, SearchRangeOption.ThisAndChildren);

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
            string key = "Key2";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            int position = 0;

            expectedValue = _treeRoot.InsertChild(position, key);

            // Verify if added
            ITreeItem<TreeNodeAdv> returnValue;
            returnValue = _treeRoot.FindItem(key, SearchRangeOption.ThisAndChildren);

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
            string key = "Key3";
            string text = "Text3";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            int position = 0;

            expectedValue = _treeRoot.InsertChild(position, key, text);

            // Verify if added
            ITreeItem<TreeNodeAdv> returnValue;
            returnValue = _treeRoot.FindItem(key, SearchRangeOption.ThisAndChildren);

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
            TreeNodeAdv node = null;
            string key = "Key4";
            string text = "Text4";
            int position = 0;

            // Declare return type to hold constructor result
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue;

            // Make method call
            expectedValue = _treeItem.InsertChild(position, node, key, text);
            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(key, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);

            returnValue = _treeRoot.FindItem(key, SearchRangeOption.RecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem.AddChild(System.Object,System.String,System.String,System.String,System.Object)")]
        [Test]
        public void VerifyInsertChildOverload5()
        {
            // Declare variables to pass to method call
            TreeNodeAdv node = new TreeNodeAdv();
            string key = "Key5";
            string text = "Text5";
            string tag = "Tag5";
            object data = new TreeNodeAdv();
            int position = 0;

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue;

            // Make method call
            expectedValue = _treeItem.InsertChild(position, node, key, text, tag, data);

            // Perform Assert Tests
            returnValue = _treeItem.FindItem(node, SearchRangeOption.ThisAndChildren);
            Assert.AreSame(expectedValue, returnValue);

            returnValue = _treeRoot.FindItem(node, SearchRangeOption.ThisAndRecursiveChildren);
            Assert.AreSame(expectedValue, returnValue);

            bool isRemoved = _treeItem.RemoveChild(node, false);

            Assert.IsTrue(isRemoved);

            returnValue = _treeItem.FindItem(node, SearchRangeOption.ThisAndChildren);
            Assert.IsNull(returnValue);
            returnValue = _treeItem.FindItem(node, SearchRangeOption.ThisAndRecursiveChildren);
            Assert.IsNull(returnValue);
        }

        /// <summary>
        /// AddChild, RemoveChild, FindChild, FindItem test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem.AddChild(System.String,System.String,System.String,System.Object)")]
        [Test]
        public void VerifyInsertChildOverload6()
        {
            // Declare variables to pass to method call
            //TODO: Set values for variables
            string key = "Key6";
            string text = "Text6";
            string tag = "Tag6";
            object data = new TreeNodeAdv();
            int position = 0;

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> expectedValue;
            ITreeItem<TreeNodeAdv> returnValue;

            // Make method call
            expectedValue = _treeItem.InsertChild(position, key, text, tag, data);
            returnValue = _treeItem.FindItem(key, SearchRangeOption.ThisAndChildren);

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
            string text = "Text1";

            // Declare return type to hold method result
            ITreeItem<TreeNodeAdv> returnValue;
            ITreeItem<TreeNodeAdv> expectedValue = null;

            // Remove with no Parent should return false
            Assert.IsFalse(_treeRoot.Remove());

            expectedValue = new TreeItem<TreeNodeAdv>(text);
            expectedValue = _treeItem.AddChild(expectedValue);

            // Make method call
            returnValue = _treeItem.FindItem(text, SearchRangeOption.ThisAndChildren);

            // Perform Assert Tests
            Assert.AreSame(expectedValue, returnValue);

            bool isRemoved = expectedValue.Remove();

            Assert.IsTrue(isRemoved);
            Assert.IsNull(_treeItem.FindItem(expectedValue, SearchRangeOption.ThisAndChildren));
        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Test the Key property.
        /// </summary>
        [Test]
        public void VerifyKey()
        {
            // Declare return variable to hold property get method
            string getValue;

            // Test get method
            getValue = _treeItem.Key;

            // Perform Assert Tests
            Assert.AreEqual(getValue, "Key");
        }

        /// <summary>
        /// Test the IsLeaf property.
        /// </summary>
        [Test]
        public void VerifyIsLeaf()
        {
            // Declare return variable to hold property get method
            bool getValue;

            // Test get method
            getValue = _treeItem.IsLeaf;

            Assert.IsFalse(getValue);

            // Declare variable to hold property set method
            bool setValue = true;

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
            ITreeItem<TreeNodeAdv> level2Item = _treeItem.AddChild("ChildToTreeItem"); 

            // Declare return variable to hold property get method
            int getValue;

            // Test get method
            getValue = _treeItem.Level;
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
            string getValue;
            string setText;

            // Test set method
            setText = "set Text";
            _treeItem.Text = setText;

            // Test get method
            getValue = _treeItem.Text;

            // Perform Assert Tests
            Assert.AreEqual(getValue, setText);
        }

        /// <summary>
        /// Test the ChildCollection property.
        /// </summary>
        [Test]
        public void VerifyChildren()
        {
            // Declare return variable to hold property get method
            ReadOnlyCollection<ITreeItem<TreeNodeAdv>> getValue = null;

            // Test get method
            getValue = _treeRoot.ChildCollection;

            // Perform Assert Tests
            Assert.AreEqual(getValue.Count, 1);
        }

        /// <summary>
        /// Test the ResursiveList property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item")]
        [Test]
        public void VerifyRecursiveChildCollection()
        {
            // Declare return variable to hold property get method
            ReadOnlyCollection<ITreeItem<TreeNodeAdv>> getValue;

            TreeItem<TreeNodeAdv> item2 = new TreeItem<TreeNodeAdv>(_treeRoot, "Key2", "Text2");

            // Test get method
            getValue = _treeRoot.RecursiveChildCollection;

            // Perform Assert Tests
            Assert.AreEqual(3, getValue.Count);

            TreeItem<TreeNodeAdv> item3 = new TreeItem<TreeNodeAdv>(_treeItem, "Key3", "Text3");
            TreeItem<TreeNodeAdv> item4 = new TreeItem<TreeNodeAdv>(item2, "Key4", "Text4");
            TreeItem<TreeNodeAdv> item5 = new TreeItem<TreeNodeAdv>(item4, "Key5", "Text5");

            // Test get method
            getValue = _treeRoot.RecursiveChildCollection;
            // Perform Assert Tests
            Assert.AreEqual(6, getValue.Count);
        }

        [Test]
        public void VerifyRecursiveParentCollection()
        {
            // Declare return variable to hold property get method
            ReadOnlyCollection<ITreeItem<TreeNodeAdv>> getValue;

            // Test get method
            getValue = _treeRoot.RecursiveParentCollection;

            // Perform Assert Tests
            Assert.AreEqual(0, getValue.Count);

            // Test get method
            getValue = _treeItem.RecursiveParentCollection;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
            Assert.AreSame(_treeRoot, getValue[0]);

            TreeItem<TreeNodeAdv> item2 = new TreeItem<TreeNodeAdv>(_treeItem, "Key2", "Text2");
            TreeItem<TreeNodeAdv> item3 = new TreeItem<TreeNodeAdv>(item2, "Key3", "Text3");
            TreeItem<TreeNodeAdv> item4 = new TreeItem<TreeNodeAdv>(item3, "Key4", "Text4");
            TreeItem<TreeNodeAdv> item5 = new TreeItem<TreeNodeAdv>(item4, "Key5", "Text5");

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

        ///// <summary>
        ///// Test the Node property.
        ///// </summary>
        [Test]
        public void VerifyNode()
        {
            // Declare return variable to hold property get method
            TreeItem<TreeNodeAdv> getValue;

            // Declare variable to hold property set method
            TreeNodeAdv setValue = new TreeNodeAdv();

            // Test set method
            _treeItem.Node = setValue;


            //TODO: Need to be remove by dinesh
            // Test get method
            getValue = new TreeItem<TreeNodeAdv>("test");

            // Perform Assert Tests
            Assert.IsInstanceOfType(typeof(TreeItem<TreeNodeAdv>), getValue);
        }

        /// <summary>
        /// Test the Tag property.
        /// </summary>
        [Test]
        public void VerifyTag()
        {
            // Declare return variable to hold property get method
            string getValue;

            // Declare variable to hold property set method
            string setValue = "Tag3";

            // Test set method
            _treeItem.DescriptionText = setValue;

            // Test get method
            getValue = _treeItem.DescriptionText;

            // Perform Assert Tests
            Assert.AreEqual(getValue, "Tag3");
        }

        ///// <summary>
        ///// Test the Data property.
        ///// </summary>
        [Test]
        public void VerifyData()
        {
            // Declare return variable to hold property get method
            object getValue = null;

            // Declare variable to hold property set method
            TreeItem<TreeNodeAdv> setValue = new TreeItem<TreeNodeAdv>("Set");           


            // Test set method
            _treeItem.Data = setValue;

            // Test get method
            getValue = _treeItem.Data;

            // Perform Assert Tests
            Assert.IsInstanceOfType(typeof(TreeItem<TreeNodeAdv>), getValue);

            


        }

        /// <summary>
        /// Test the ImageIndex property.
        /// </summary>
        [Test]
        public void VerifyImageIndex()
        {
            // Declare return variable to hold property get method
            int getValue;

            // Declare variable to hold property set method
            int setValue = 2;

            // Test set method
            _treeItem.ImageIndex = setValue;

            // Test get method
            getValue = _treeItem.ImageIndex;

            // Perform Assert Tests
            Assert.AreEqual(getValue, setValue);

            // set back to -1
            setValue = -1;

            _treeItem.ImageIndex = setValue;
            _treeRoot.ImageIndex = setValue;

            // add new 'not leaf' and 'leaf' items to _treeItem
            TreeItem<TreeNodeAdv> leafItem = new TreeItem<TreeNodeAdv>("LeafItem");
            leafItem.IsLeaf = true;
            TreeItem<TreeNodeAdv> notLeafItem = new TreeItem<TreeNodeAdv>("NotLeafItem");
            notLeafItem.IsLeaf = false;

            _treeItem.AddChild(leafItem);
            _treeItem.AddChild(notLeafItem);

            Assert.AreEqual(0, _treeRoot.ImageIndex);
            Assert.AreEqual(1, notLeafItem.ImageIndex);
            Assert.AreEqual(2, leafItem.ImageIndex);
        }

        /// <summary>
        /// Test the Parent property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String,System.String)")]
        [Test]
        public void VerifyParent()
        {
            // Declare return variable to hold property get method
            ITreeItem<TreeNodeAdv> getValue;
            int oldChildValue = _treeRoot.ChildCollection.Count;

            // Declare return type to hold constructor result
            TreeItem<TreeNodeAdv> newValue = new TreeItem<TreeNodeAdv>("Key2", "Text2");

            // Test set method
            newValue.Parent = _treeRoot;

            // Test get method
            getValue = newValue.Parent;

            // Perform Assert Tests
            Assert.AreSame(_treeRoot, getValue);

            // Has only this child once
            Assert.AreEqual(oldChildValue + 1, _treeRoot.ChildCollection.Count);
        }

        /// <summary>
        /// Test the HasChildren property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "_newItem")]
        [Test]
        public void VerifyHasChildren()
        {
            // Declare return variable to hold property get method
            bool getValue;

            // Test get method
            getValue = _treeItem.HasChildren;

            Assert.IsFalse(getValue);

            // Perform Assert Tests
            TreeItem<TreeNodeAdv> _newItem = new TreeItem<TreeNodeAdv>(_treeItem, "Key3", "Text3");

            // Test get method
            getValue = _treeItem.HasChildren;

            Assert.IsTrue(getValue);
        }

        /// <summary>
        /// Test the HasParent property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String,System.String)")]
        [Test]
        public void VerifyHasParent()
        {
            // Declare return variable to hold property get method
            bool getValue;

            // Perform Assert Tests
            TreeItem<TreeNodeAdv> newItem = new TreeItem<TreeNodeAdv>("Key3", "Text3");

            // Test get method
            getValue = newItem.HasParent;

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
            // Declare return variable to hold property get method
            object getValue = null;

            // Declare variable to hold property set method
            ITreeNodeDisplayInfo setValue = new TreeNodeAdvDisplayInfo(new TreeNodeAdv());

            // Test set method
            _treeItem.NodeDisplayInfo = setValue;

            // Test get method
            getValue = _treeItem.NodeDisplayInfo;

            // Perform Assert Tests
            Assert.AreSame(setValue, getValue);
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String,System.String)")]
        //[Test]
        //public void VerifySaveNodeDisplayInfo()
        //{
            

        //    // test nodeinfo with no node

        //    Assert.IsNull(_treeRoot.NodeDisplayInfo);

        //    _treeRoot.SaveNodeDisplayInfo();

        //    Assert.IsNull(_treeRoot.NodeDisplayInfo);

        //    // create nodes and test with them

        //    _treeRoot.Node = new TreeNodeAdv("ROOT");

        //    TreeItemAdv item1 = new TreeItemAdv(_treeRoot, "ITEM1");
        //    item1.Node = new TreeNodeAdv("ITEM1");
        //    TreeItemAdv item2 = new TreeItemAdv(_treeRoot, "ITEM2");
        //    item2.Node = new TreeNodeAdv("ITEM2");

        //    Assert.IsNull(item1.NodeDisplayInfo);
        //    Assert.IsNull(item2.NodeDisplayInfo);

        //    //_treeRoot.SaveNodeDisplayInfo();

        //    //Assert.IsNotNull(_treeRoot.NodeDisplayInfo);
        //    //Assert.IsNotNull(item1.NodeDisplayInfo);
        //    //Assert.IsNotNull(item2.NodeDisplayInfo);

        //}

        #endregion
    }
}

