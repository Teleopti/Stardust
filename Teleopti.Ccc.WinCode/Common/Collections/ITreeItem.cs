using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Item range enumeration
    /// </summary>
    public enum RangeOption
    {
        /// <summary>
        /// Range of this item only.
        /// </summary>
        ThisOnly,
        /// <summary>
        /// Range of the children.
        /// </summary>
        Children,
        /// <summary>
        /// Range of this item and all in children.
        /// </summary>
        ThisAndChildren,
        /// <summary>
        /// Range of the children recursively.
        /// </summary>
        RecursiveChildren,
        /// <summary>
        /// Range of this item and all in the children recursively.
        /// </summary>
        ThisAndRecursiveChildren,
    }

    /// <summary>
    /// Gives information about the selected status of the
    /// current item.
    /// </summary>
    public enum SelectedChildrenOption
    {
        /// <summary>
        /// None of the children are selected in branch
        /// </summary>
        NoneSelected,
        /// <summary>
        /// Some of the children are selected in branch
        /// </summary>
        SomeSelected,
        /// <summary>
        /// Every children are selected in branch
        /// </summary>
        AllSelected
    }

    /// <summary>
    /// Smart parent select option.
    /// </summary>
    public enum AutoParentSelectionOption
    {
        /// <summary>
        /// Switched off
        /// </summary>
        None,
        /// <summary>
        /// Selects the parent node if all childnodes are selected
        /// </summary>
        SelectParentIfAllChildSelected,
        /// <summary>
        /// Selects the parent node if at least one childnode is selected
        /// </summary>
        SelectParentIfAtLeastOneChildSelected
    }

    /// <summary>
    /// Summary description for ITreeItem.
    /// </summary>
    public interface ITreeItem<TNode>
    {

        /// <summary>
        /// Removing event handler. Fired before removing an item from collection.
        /// </summary>
        event EventHandler<TreeItemEventArgs<TNode>> Removing;

        /*
        /// <summary>
        /// Removing event handler. Fired before marking an item as deleted.
        /// </summary>
        event EventHandler<TreeItemEventArgs> Deleting;
        */
   
        /// <summary>
        /// Adding event handler. Fired before adding a new item.
        /// </summary>
        event EventHandler<TreeItemEventArgs<TNode>> Adding;

        /// <summary>
        /// Adding event handler. Fired after removing an item.
        /// </summary>
        event EventHandler<TreeItemEventArgs<TNode>> Removed;

        /*
        /// <summary>
        /// Adding event handler. Fired after marking an item as deleted.
        /// </summary>
        event EventHandler<TreeItemEventArgs> Deleted;
        */

        /// <summary>
        /// Adding event handler. Fired after adding a new item.
        /// </summary>
        event EventHandler<TreeItemEventArgs<TNode>> Added;

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(string key, string text);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(string text);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The Node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(TNode node, string key, string text);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(string key, string text, string tag, object data);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(TNode node, string key, string text, string description, object data);

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        ITreeItem<TNode> AddChild(ITreeItem<TNode> item);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> InsertChild(int index, string key, string text);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> InsertChild(int index, string text);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ITreeItem<TNode> InsertChild(int index, TNode node, string key, string text);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ITreeItem<TNode> InsertChild(int index, string key, string text, string description, object data);

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
        ITreeItem<TNode> InsertChild(int index, TNode node, string key, string text, string description, object data);

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        ITreeItem<TNode> InsertChild(int index, ITreeItem<TNode> item);

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
        /// <param name="node">The node.</param>
        /// <param name="recursiveSearch">if set to <c>true</c> searches for the child recursively.</param>
        /// <returns></returns>
        bool RemoveChild(TNode node, bool recursiveSearch);

        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool RemoveChild(ITreeItem<TNode> item);

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <returns></returns>
        bool Remove();

        /// <summary>
        /// Finds a item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        ITreeItem<TNode> FindItem(TNode node, RangeOption range);

        /// <summary>
        /// Finds a item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        ITreeItem<TNode> FindItem(string key, RangeOption range);

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
        ITreeItem<TNode> FindItem(object data, RangeOption range);

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
        ITreeItem<TNode> FindItem(ITreeItem<TNode> treeItem, RangeOption range);

        /// <summary>
        /// Enumerate the item and its children according to the Range option.
        /// </summary>
        /// <param name="range">The search range.</param>
        /// <returns></returns>
        IEnumerable<ITreeItem<TNode>> Enumerate(RangeOption range);
            
        /// <summary>
        /// Creates a absolut key from the item key and the parents keys.
        /// </summary>
        /// <value>The strong key.</value>
        string AbsoluteKey { get; }

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
        ReadOnlyCollection<ITreeItem<TNode>> ChildCollection { get; }

        /// <summary>
        /// Gets a recursive collection of all the children.
        /// </summary>
        /// <value>The recursive list.</value>
        ReadOnlyCollection<ITreeItem<TNode>> RecursiveChildCollection { get; }

        /// <summary>
        /// Gets a recursive collection of all the parents (ancessors).
        /// </summary>
        /// <value>The recursive list of parents.</value>
        ReadOnlyCollection<ITreeItem<TNode>> RecursiveParentCollection { get; }

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
        ITreeItem<TNode> Parent { get; set; }

        /// <summary>
        /// Gets or sets the rootNode.
        /// </summary>
        /// <value>The rootNode.</value>
        TNode Node { get; set; }

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

        /// <summary>
        /// Gets or sets the data that is connected with the visualization of the made.
        /// </summary>
        /// <value>The node display data.</value>
        object NodeDisplayInfo { get; set; }

        /// <summary>
        /// Gets the selected status of the children.
        /// </summary>
        /// <value>The selected status of the children.</value>
        SelectedChildrenOption SelectedChildren { get; }

        /// <summary>
        /// Selecteds this instance.
        /// </summary>
        bool Selected{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the children of the item are selected/unselected.
        /// </summary>
        /// <value><c>true</c> if the item has auto child selection; otherwise, <c>false</c>.</value>
        bool AutoChildSelection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the children of the item are selected/unselected recursively.
        /// </summary>
        /// <value>The recursive select range.</value>
        AutoParentSelectionOption AutoParentSelection { get; set; }

        /// <summary>
        /// Runs the auto select procedure on the children.
        /// </summary>
        void AutoSelectChildren();

        /// <summary>
        /// Runs the auto select procedure on the parents.
        /// </summary>
        void AutoSelectParent();

        /// <summary>
        /// Provides the dictionary for the stored data.
        /// </summary>
        /// <value>The stored data.</value>
        Dictionary<string, object> StoredDataDictionary { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is permitted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is permitted; otherwise, <c>false</c>.
        /// </value>
        bool IsPermitted { get; set; }
    }
}