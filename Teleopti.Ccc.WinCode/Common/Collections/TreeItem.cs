using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// TreeItem object in the collection
    /// </summary>
    [Serializable]
    public class TreeItem<TNode> : ITreeItem<TNode>
    {
        #region Variables

        /// <summary>
        /// Removing event handler. Fired before removing an item from collection.
        /// </summary>
        public event EventHandler<TreeItemEventArgs<TNode>> Removing;

        //public event EventHandler<TreeItemEventArgs> Deleting;

        /// <summary>
        /// Adding event handler. Fired before adding a new item.
        /// </summary>
        public event EventHandler<TreeItemEventArgs<TNode>> Adding;

        /// <summary>
        /// Adding event handler. Fired after removing an item.
        /// </summary>
        public event EventHandler<TreeItemEventArgs<TNode>> Removed;

        //public event EventHandler<TreeItemEventArgs> Deleted;

        /// <summary>
        /// Adding event handler. Fired after adding a new item.
        /// </summary>
        public event EventHandler<TreeItemEventArgs<TNode>> Added;

        private readonly Dictionary<string, object> _storedData;
        private const string StoredNodeKey = "StoredNodeKey";
        private const string StoredDataKey = "StoredDataKey";
        private const string StoredDisplayInfoKey = "StoredDisplayInfoKey";

        private readonly string _key;
        private string _text;
        private readonly IList<ITreeItem<TNode>> _children;
        private string _description;
        private bool _isLeaf;
        private int _imageIndex = -1;
        private ITreeItem<TNode> _parent;
        private bool _selected;
        private bool _autoChildSelection;
        private AutoParentSelectionOption _autoParentSelection = AutoParentSelectionOption.None;
        private bool _isPermitted = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItem(ITreeItem<TNode> parent, string key, string text, string description, object data)
        {
            _key = key;
            _text = text;
            _description = description;
            _children = new List<ITreeItem<TNode>>();
            _storedData = new Dictionary<string, object>(5);
            _storedData.Add(StoredNodeKey, null);
            _storedData.Add(StoredDataKey, data);
            Parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItem(ITreeItem<TNode> parent, TNode node, string key, string text, string description, object data)
            : this(parent, key, text, description, data)
        {
            _storedData[StoredNodeKey] = node;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItem(TNode node, string key, string text, string description, object data)
            : this(null, node, key, text, description, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItem(string key, string text, string description, object data)
            : this(null, key, text, description, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItem(ITreeItem<TNode> parent, TNode node, string key, string text)
            : this(parent, node, key, text, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItem(TNode node, string key, string text)
            : this(null, node, key, text, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItem(ITreeItem<TNode> parent, string key, string text)
            : this(parent, key, text, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItem(string key, string text)
            : this(null, key, text, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="text">The text.</param>
        public TreeItem(ITreeItem<TNode> parent, string text)
            : this(parent, text, text, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem{TNode}"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public TreeItem(string text)
            : this(null, text, text, null, null)
        {
        }

        #endregion

        #region Interface

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leaf.
        /// </summary>
        /// <value><c>true</c> if this instance is leaf; otherwise, <c>false</c>.</value>
        public bool IsLeaf
        {
            get { return _isLeaf; }
            set { _isLeaf = value; }
        }

        /// <summary>
        /// Gets or sets the level or depth of the instance in the collection.
        /// </summary>
        /// <value>The level.</value>
        /// <remarks>
        /// The root node level is 0.
        /// </remarks>
        public int Level
        {
            get
            {
                if (Parent == null) return 0;
                return (1 + Parent.Level);
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets the childrens list.
        /// </summary>
        /// <value>The children.</value>
        public ReadOnlyCollection<ITreeItem<TNode>> ChildCollection
        {
            get { return new ReadOnlyCollection<ITreeItem<TNode>>(_children); }
        }

        /// <summary>
        /// Gets a recursive list of this item plus all the children.
        /// </summary>
        /// <value>The recursive list.</value>
        public ReadOnlyCollection<ITreeItem<TNode>> RecursiveChildCollection
        {
            get
            {
                IList<ITreeItem<TNode>> list = new List<ITreeItem<TNode>>();
                list.Add(this);
                foreach (ITreeItem<TNode> item in ChildCollection)
                {
                    IList<ITreeItem<TNode>> subList = item.RecursiveChildCollection;
                    foreach (ITreeItem<TNode> subItem in subList)
                    {
                        list.Add(subItem);
                    }
                }
                return new ReadOnlyCollection<ITreeItem<TNode>>(list);
            }
        }

        /// <summary>
        /// Gets a recursive collection of all the parents (ancessors).
        /// </summary>
        /// <value>The recursive list of parents.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/13/2007
        /// </remarks>
        public ReadOnlyCollection<ITreeItem<TNode>> RecursiveParentCollection
        {
            get
            {
                IList<ITreeItem<TNode>> list = new List<ITreeItem<TNode>>();
                if (Parent != null)
                {
                    foreach (ITreeItem<TNode> item in Parent.RecursiveParentCollection)
                    {
                        list.Add(item);
                    }
                    list.Add(Parent);
                }
                return new ReadOnlyCollection<ITreeItem<TNode>>(list);

            }
        }

        /// <summary>
        /// Gets or sets the tree rootNode.
        /// </summary>
        /// <value>The rootNode.</value>
        public TNode Node
        {
            get { return (TNode)_storedData[StoredNodeKey]; }
            set { _storedData[StoredNodeKey] = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string DescriptionText
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets an extra data.
        /// </summary>
        /// <value>The data.</value>
        public object Data
        {
            get { return _storedData[StoredDataKey]; }
            set { _storedData[StoredDataKey] = value; }
        }

        /// <summary>
        /// Gets or sets the index of the image.
        /// </summary>
        /// <value>The index of the image of the rootNode.</value>
        /// <remarks>
        /// If imageindex is -1 then by default in runtime the rootItem item will get an
        /// imageindex of 0, the not leaf items will get a value of 1 and the leaf items will
        /// get a value of 2.
        /// </remarks>
        public int ImageIndex
        {
            get { return DefineDefaultImageIndex(); }
            set { _imageIndex = value; }
        }

        /// <summary>
        /// Gets or defines the image index.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// By default, it will return an index of 0 for the
        /// rootItem item, 1 if the item is not leaf, and 2 if the item is leaf.
        /// Created by: tamasb
        /// Created date: 11/15/2007
        /// </remarks>
        protected int DefineDefaultImageIndex()
        {
            if (_imageIndex != -1) return _imageIndex;
            if (!HasParent) return 0;
            if (!IsLeaf) return 1;
            
            return 2;
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ITreeItem<TNode> Parent
        {
            get { return _parent; }
            set { setParent(value); }
        }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-09
        /// </remarks>
        private void setParent(ITreeItem<TNode> parent)
        {
            if (parent != null)
            {
                _parent = parent;
                _parent.AddChild(this);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren
        {
            get { return (ChildCollection.Count > 0); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has parent.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has parent; otherwise, <c>false</c>.
        /// </value>
        public bool HasParent
        {
            get { return (Parent != null); }
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public ITreeItem<TNode> InsertChild(int index, ITreeItem<TNode> item)
        {
            if (item != null)
            {
                TreeItemEventArgs<TNode> args = OnAdding(item);
                if (!args.Cancel)
                {
                    _children.Insert(index, item);
                    if (!item.HasParent)
                        item.Parent = this;
                    OnAdded(item);
                    return item;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> InsertChild(int index, string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(text);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> InsertChild(int index, string key, string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(key, text);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> InsertChild(int index, TNode node, string key, string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(node, key, text);
            return InsertChild(index, item);
        }

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
        public ITreeItem<TNode> InsertChild(int index, TNode node, string key, string text, string description, object data)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(node, key, text, description, data);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ITreeItem<TNode> InsertChild(int index, string key, string text, string description, object data)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(key, text, description, data);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(ITreeItem<TNode> item)
        {
            if (item != null && FindItem(item, RangeOption.Children) == null)
            {
                TreeItemEventArgs<TNode> args = OnAdding(item);
                if (!args.Cancel)
                {
                    _children.Add(item);
                    if (!item.HasParent)
                        item.Parent = this;
                    OnAdded(item);
                    return item;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(this, text);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(string key, string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(this, key, text);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The Node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(TNode node, string key, string text)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(this, node, key, text);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(TNode node, string key, string text, string description, object data)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(this, node, key, text, description, data);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ITreeItem<TNode> AddChild(string key, string text, string tag, object data)
        {
            ITreeItem<TNode> item = new TreeItem<TNode>(this, key, text, tag, data);
            return item;
        }

        /// <summary>
        /// Enumerate the item and its children according to the Range option.
        /// </summary>
        /// <param name="range">The search range.</param>
        /// <returns></returns>
        public IEnumerable<ITreeItem<TNode>> Enumerate(RangeOption range)
        {
            if (range == RangeOption.ThisAndChildren || range == RangeOption.ThisAndRecursiveChildren || range == RangeOption.ThisOnly)
            {
                yield return this;
            }
            if (range != RangeOption.ThisOnly)
            {
                foreach (ITreeItem<TNode> item in ChildCollection)
                {
                    if (range == RangeOption.Children || range == RangeOption.ThisAndChildren)
                    {
                        foreach (ITreeItem<TNode> subItem in item.Enumerate(RangeOption.ThisOnly))
                        {
                            yield return subItem;
                        }
                    }
                    else
                    {
                        foreach (ITreeItem<TNode> subItem in item.Enumerate(RangeOption.ThisAndRecursiveChildren))
                        {
                            yield return subItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="range">The search range.</param>
        /// <returns></returns>
        public ITreeItem<TNode> FindItem(string key, RangeOption range)
        {
            foreach (ITreeItem<TNode> subItem in Enumerate(range))
            {
                if (subItem.Key==key)
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public ITreeItem<TNode> FindItem(TNode node, RangeOption range)
        {
            if (node == null)
                return null;
            foreach (ITreeItem<TNode> subItem in Enumerate(range))
            {
                if (subItem.Node != null && subItem.Node.Equals(node))
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-08
        /// </remarks>
        public ITreeItem<TNode> FindItem(object data, RangeOption range)
        {
            if (data == null)
                return null;
            foreach (ITreeItem<TNode> subItem in Enumerate(range))
            {
                if (subItem.Data == data)
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="treeItem">The item.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public ITreeItem<TNode> FindItem(ITreeItem<TNode> treeItem, RangeOption range)
        {
            if (treeItem == null)
                return null;
            foreach (ITreeItem<TNode> subItem in Enumerate(range))
            {
                if (subItem == treeItem)
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Creates a absolut key from the item key and the parents keys.
        /// </summary>
        /// <value>The strong key.</value>
        public string AbsoluteKey
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (ITreeItem<TNode> item in RecursiveParentCollection)
                {
                    builder.Append(item.Key);
                }
                builder.Append(Key);
                return builder.ToString();
            }
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Remove()
        {
            if (Parent != null)
                return Parent.RemoveChild(this);
            
            return false;
        }

        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="recursiveSearch">if set to <c>true</c> [recursive search].</param>
        /// <returns></returns>
        public bool RemoveChild(string key, bool recursiveSearch)
        {
            RangeOption range = RangeOption.ThisOnly;
            if (recursiveSearch)
                range = RangeOption.ThisAndRecursiveChildren;
            foreach (ITreeItem<TNode> child in ChildCollection)
            {
                ITreeItem<TNode> item = child.FindItem(key, range);
                if (item != null)
                    return item.Remove();
            }
            return false;
        }

        /// <summary>
        /// Removes a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="recursiveSearch">if set to <c>true</c> [recursive search].</param>
        /// <returns></returns>
        public bool RemoveChild(TNode node, bool recursiveSearch)
        {
            RangeOption range = RangeOption.ThisOnly;
            if (recursiveSearch)
                range = RangeOption.ThisAndRecursiveChildren;
            foreach (ITreeItem<TNode> child in ChildCollection)
            {
                ITreeItem<TNode> item = child.FindItem(node, range);
                if (item != null)
                    return item.Remove();
            }
            return false;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool RemoveChild(ITreeItem<TNode> item)
        {
            if (item == null)
                return false;
            TreeItemEventArgs<TNode> args = OnRemoving(item);
            if (args.Cancel == false)
            {
                if (_children.Remove(item))
                {
                    OnRemoved(item);
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets the selected status.
        /// </summary>
        /// <value>The selected status.</value>
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                AutoSelectChildren();
                AutoSelectParent();
            }
        }

        /// <summary>
        /// Gets the selected status of the children.
        /// </summary>
        /// <value>The selected status of the children.</value>
        public SelectedChildrenOption SelectedChildren
        {
            get
            {
                if (!HasChildren)
                    return SelectedChildrenOption.NoneSelected;

                bool allChildrenSelected = true;
                bool atLeastOneChildSelected = false;
                foreach (ITreeItem<TNode> item in Enumerate(RangeOption.Children))
                {
                    allChildrenSelected = (allChildrenSelected && item.Selected);
                    atLeastOneChildSelected = (atLeastOneChildSelected || item.Selected);
                }
                if (allChildrenSelected)
                    return SelectedChildrenOption.AllSelected;
                if (atLeastOneChildSelected)
                    return SelectedChildrenOption.SomeSelected;
                
                return SelectedChildrenOption.NoneSelected;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the children of the item are selected/unselected recursively.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the select children option is ON; otherwise, <c>false</c>.
        /// </value>
        public bool AutoChildSelection
        {
            get
            {
                return _autoChildSelection;
            }
            set
            {
                _autoChildSelection = value;
                foreach (ITreeItem<TNode> item in Enumerate(RangeOption.RecursiveChildren))
                {
                    item.AutoChildSelection = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the children of the item are selected/unselected recursively.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the select children recursively option is ON; otherwise, <c>false</c>.
        /// </value>
        public AutoParentSelectionOption AutoParentSelection
        {
            get
            {
                return _autoParentSelection;
            }
            set
            {
                _autoParentSelection = value;
                foreach (ITreeItem<TNode> item in Enumerate(RangeOption.RecursiveChildren))
                {
                    item.AutoParentSelection = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the data that is connected with the visualization.
        /// </summary>
        /// <value>The node display data.</value>
        public object NodeDisplayInfo
        {
            get { return _storedData.ContainsKey(StoredDisplayInfoKey) ? _storedData[StoredDisplayInfoKey] : null; }
            set
            {
                if (_storedData.ContainsKey(StoredDisplayInfoKey))
                    _storedData[StoredDisplayInfoKey] = value;
                else
                    _storedData.Add(StoredDisplayInfoKey, value);
            }
        }

        /// <summary>
        /// Provides the dictionary for the stored data.
        /// </summary>
        /// <value>The stored data.</value>
        public Dictionary<string, object> StoredDataDictionary
        {
            get { return _storedData; }
        }

        /// <summary>
        /// Runs the auto select procedure on the children.
        /// </summary>
        public void AutoSelectChildren()
        {
            if (AutoChildSelection)
            {
                bool value = Selected; 
                foreach (ITreeItem<TNode> item in Enumerate(RangeOption.Children))
                {
                    item.Selected = value;
                }
            }
        }

        /// <summary>
        /// Runs the auto select procedure on the parents.
        /// </summary>
        public void AutoSelectParent()
        {
            if (HasChildren)
            {
                switch (AutoParentSelection)
                {
                    case AutoParentSelectionOption.SelectParentIfAllChildSelected:
                        _selected = (SelectedChildren == SelectedChildrenOption.AllSelected);
                        break;
                    case AutoParentSelectionOption.SelectParentIfAtLeastOneChildSelected:
                        _selected = (SelectedChildren >= SelectedChildrenOption.SomeSelected);
                        break;
                    default:
                        break;
                }
            }
            if (Parent != null)
                Parent.AutoSelectParent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is permitted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is permitted; otherwise, <c>false</c>.
        /// </value>
        public bool IsPermitted
        {
            get { return _isPermitted; }
            set { _isPermitted = value; }
        }

        #endregion

        #region OnEvent methods

        /// <summary>
        /// Fires the Removing event.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected virtual TreeItemEventArgs<TNode> OnRemoving(ITreeItem<TNode> item)
        {
            TreeItemEventArgs<TNode> args = new TreeItemEventArgs<TNode>(item, _parent);
            if (Removing != null)
                Removing(this, args);
            return args;
        }

        /// <summary>
        /// Fires the Adding event.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected virtual TreeItemEventArgs<TNode> OnAdding(ITreeItem<TNode> item)
        {
            TreeItemEventArgs<TNode> args = new TreeItemEventArgs<TNode>(item, this);
            if (Adding != null)
                Adding(this, args);
            return args;
        }

        /// <summary>
        /// Fires the Removed event.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnRemoved(ITreeItem<TNode> item)
        {
            TreeItemEventArgs<TNode> args = new TreeItemEventArgs<TNode>(item, _parent);
            if (Removed != null)
                Removed(this, args);
        }

        /// <summary>
        /// Fires the Added event.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnAdded(ITreeItem<TNode> item)
        {
            TreeItemEventArgs<TNode> args = new TreeItemEventArgs<TNode>(item, this);
            if (Added != null)
                Added(this, args);
        }

        #endregion

    }
}