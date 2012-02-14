using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Summary description for ITreeItem.
    /// </summary>    
    [CLSCompliant(false)]
    public interface ITreeItemAdv
    {

        /// <summary>
        /// Removing event handler. Fired before removing an item from collection.
        /// </summary>
        event EventHandler<TreeItemAdvEventArgs> Removing;

        /*
        /// <summary>
        /// Removing event handler. Fired before marking an item as deleted.
        /// </summary>
        event EventHandler<TreeItemEventArgs> Deleting;
        */
   
        /// <summary>
        /// Adding event handler. Fired before adding a new item.
        /// </summary>
        event EventHandler<TreeItemAdvEventArgs> Adding;

        /// <summary>
        /// Adding event handler. Fired after removing an item.
        /// </summary>
        event EventHandler<TreeItemAdvEventArgs> Removed;

        /*
        /// <summary>
        /// Adding event handler. Fired after marking an item as deleted.
        /// </summary>
        event EventHandler<TreeItemEventArgs> Deleted;
        */

        /// <summary>
        /// Adding event handler. Fired after adding a new item.
        /// </summary>
        event EventHandler<TreeItemAdvEventArgs> Added;

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(string key, string text);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(string text);


        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(string key, string text, string tag, object data);


        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(ITreeItemAdv item);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, string key, string text);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, string text);


        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, string key, string text, string description, object data);


        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, ITreeItemAdv item);

        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="recursiveSearch">if set to <c>true</c> searches for the child recursively.</param>
        /// <returns></returns>
        bool RemoveChild(string key, bool recursiveSearch);


        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool RemoveChild(ITreeItemAdv item);

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <returns></returns>
        bool Remove();



        /// <summary>
        /// Finds a item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        ITreeItemAdv FindItem(string key, SearchRangeOption range);

        /// <summary>
        /// Finds the item.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-08
        /// </remarks>
        ITreeItemAdv FindItem(object data, SearchRangeOption range);

        /// <summary>
        /// Finds the item.
        /// </summary>
        /// <param name="treeItem">The tree item.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-08
        /// </remarks>
        ITreeItemAdv FindItem(ITreeItemAdv treeItem, SearchRangeOption range);

        /// <summary>
        /// Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        bool HasChildren { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has parent.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has parent; otherwise, <c>false</c>.
        /// </value>
        bool HasParent { get; }

        /// <summary>
        /// Gets the childrens list.
        /// </summary>
        /// <value>The children.</value>
        ReadOnlyCollection<ITreeItemAdv> ChildCollection { get; }

        /// <summary>
        /// Gets a recursive collection of all the children.
        /// </summary>
        /// <value>The recursive list.</value>
        ReadOnlyCollection<ITreeItemAdv> RecursiveChildCollection { get; }

        /// <summary>
        /// Gets a recursive collection of all the parents (ancessors).
        /// </summary>
        /// <value>The recursive list of parents.</value>
        ReadOnlyCollection<ITreeItemAdv> RecursiveParentCollection { get; }

        /// <summary>
        /// Gets or sets the index of the image.
        /// </summary>
        /// <value>The index of the image.</value>
        int ImageIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leaf.
        /// </summary>
        /// <value><c>true</c> if this instance is leaf; otherwise, <c>false</c>.</value>
        bool IsLeaf { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        ITreeItemAdv Parent { get; set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        string Key { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        string Text { get; set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        string DescriptionText { get; set; }

        /// <summary>
        /// Gets or sets an extra data.
        /// </summary>
        /// <value>The data.</value>
        object Data { get; set; }

        /// <summary>
        /// Gets the level or depth of the instance in the collection.
        /// </summary>
        /// <value>The level.</value>
        int Level { get; }

        ///// <summary>
        ///// Joins with another ITreeItem.
        ///// </summary>
        ///// <param name="master">The master item to take the data from.</param>
        ///// <remarks>
        ///// It only gets the data part from the master, but not the visualization part.
        ///// </remarks>
        //void JoinWith(ITreeItem master);

        /// <summary>
        /// Gets or sets the data that is connected with the visualization.
        /// </summary>
        /// <value>The node display data.</value>
        object NodeDisplayInfo { get; set; }

        /// <summary>
        /// Saves the node display info.
        /// </summary>
        void SaveNodeDisplayInfo();

        /// <summary>
        /// Gets or sets the rootNode.
        /// </summary>
        /// <value>The rootNode.</value>
        TreeNodeAdv Node { get; set; }

        /// <summary>
        /// Finds a item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        ITreeItemAdv FindItem(TreeNodeAdv node, SearchRangeOption range);


        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="recursiveSearch">if set to <c>true</c> searches for the child recursively.</param>
        /// <returns></returns>
        bool RemoveChild(TreeNodeAdv node, bool recursiveSearch);


        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, TreeNodeAdv node, string key, string text, string description, object data);


        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv InsertChild(int index, TreeNodeAdv node, string key, string text);


        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(TreeNodeAdv node, string key, string text, string description, object data);


        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The Node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItemAdv AddChild(TreeNodeAdv node, string key, string text);
    }
}