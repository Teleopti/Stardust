using NUnit.Framework;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
   /// <summary>
   /// Test cases for TreeItemEventArgs
   /// </summary>
   [TestFixture]
   public class TreeItemEventArgsWithTreeNodeAdvTest
   {

      #region Variables

       private TreeItem<TreeNodeAdv> m_treeRoot;
       private TreeItem<TreeNodeAdv> m_treeItem;
       private TreeItemEventArgs<TreeNodeAdv> m_treeItemEventArgs;

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
      /// Setup test cases
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Teleopti.Ccc.Core.Collections.TreeCollections.TreeItem.#ctor(Teleopti.Ccc.Core.Collections.TreeCollections.ITreeItem,System.Object,System.String,System.String,System.String,System.Object)")]
      [SetUp]
      public void TestInit()
      {
          m_treeRoot = new TreeItem<TreeNodeAdv>("Root");
          m_treeItem = new TreeItem<TreeNodeAdv>(m_treeRoot, null, "Key", "Text", "Tag", null);
          m_treeItemEventArgs = new TreeItemEventArgs<TreeNodeAdv>(m_treeItem, m_treeRoot);
      }


      /// <summary>
      /// Tear up testcases.
      /// </summary>
      [TearDown]
      public void TestDispose()
      {
         m_treeItemEventArgs = null;
      }

      #endregion

      #region Constructor Tests

      /// <summary>
      /// Tests constructor
      /// </summary>
      [Test]
      public void TreeItemEventArgs()
      {
         // Perform Assert Tests
         Assert.IsNotNull(m_treeItemEventArgs);
      }

      #endregion

      #region Method Tests

      #endregion

      #region Property Tests

      /// <summary>
      /// Tests ParentItem.
      /// </summary>
      [Test]
      public void ParentItem()
      {
         // Declare return variable to hold property get method
          ITreeItem<TreeNodeAdv> _getValue;

         // Test get method
         _getValue = m_treeItemEventArgs.ParentItem;

         // Perform Assert Tests
         Assert.AreSame(_getValue, m_treeRoot);

      }

      /// <summary>
      /// Tests TreeItem.
      /// </summary>
      [Test]
      public void TreeItem()
      {
         // Declare return variable to hold property get method
          ITreeItem<TreeNodeAdv> _getValue;

         // Test get method
         _getValue = m_treeItemEventArgs.TreeItem;

         // Perform Assert Tests
         Assert.AreSame(_getValue, m_treeItem);
      }

      /// <summary>
      /// Tests Cancel property.
      /// </summary>
      [Test]
      public void Cancel()
      {
         // Declare return variable to hold property get method
         bool _getValue;

         // Test get method
         _getValue = m_treeItemEventArgs.Cancel;

         // Assert
         Assert.IsFalse(_getValue);

         // Declare variable to hold property set method
         bool _setValue = true;

         // Test set method
         m_treeItemEventArgs.Cancel = _setValue;

         // Test get method
         _getValue = m_treeItemEventArgs.Cancel;

         // Assert
         Assert.IsTrue(_getValue);
      }

      #endregion

   }
}
