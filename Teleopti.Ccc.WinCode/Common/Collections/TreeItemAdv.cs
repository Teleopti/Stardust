using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// TreeItem object in the collection
    /// </summary>
    [Serializable()]
    [CLSCompliant(false)]
    public class TreeItemAdv : ITreeItemAdv
    {

        #region Private Variables

        private TreeNodeAdv _node;
        private readonly string _key;
        private string _text;
        private readonly IList<ITreeItemAdv> _children;
        private string _description;
        private object _data;
        private bool _isLeaf;
        private int _imageIndex = -1;
        private ITreeItemAdv _parent;
        private object _nodeDisplayData; 

        #endregion

        #region ITreeItemAdv Members

        /// <summary>
        /// Removing event handler. Fired before removing an item from collection.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-05
        /// </remarks>
        public event EventHandler<TreeItemAdvEventArgs> Removing;

        /// <summary>
        /// Adding event handler. Fired before adding a new item.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-05
        /// </remarks>
        public event EventHandler<TreeItemAdvEventArgs> Adding;

        /// <summary>
        /// Adding event handler. Fired after removing an item.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-05
        /// </remarks>
        public event EventHandler<TreeItemAdvEventArgs> Removed;

        /// <summary>
        /// Adding event handler. Fired after adding a new item.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-05
        /// </remarks>
        public event EventHandler<TreeItemAdvEventArgs> Added;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItemAdv(ITreeItemAdv parent, TreeNodeAdv node, string key, string text, string description, object data)
        {
            _node = node;
            _key = key;
            _text = text;
            _description = description;
            _data = data;
            _children = new List<ITreeItemAdv>();
            this.Parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItemAdv(TreeNodeAdv node, string key, string text, string description, object data)
            : this(null, node, key, text, description, data)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItemAdv(ITreeItemAdv parent, string key, string text, string description, object data)
            : this(parent, null, key, text, description, data)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="data">The data.</param>
        public TreeItemAdv(string key, string text, string description, object data)
            : this(null, null, key, text, description, data)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItemAdv(ITreeItemAdv parent, TreeNodeAdv node, string key, string text)
            : this(parent, node, key, text, null, null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItemAdv(TreeNodeAdv node, string key, string text)
            : this(null, node, key, text, null, null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItemAdv(ITreeItemAdv parent, string key, string text)
            : this(parent, null, key, text, null, null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        public TreeItemAdv(string key, string text)
            : this(null, null, key, text, null, null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="text">The text.</param>
        public TreeItemAdv(ITreeItemAdv parent, string text)
            : this(parent, null, text, text, null, null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdv"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public TreeItemAdv(string text)
            : this(null, null, text, text, null, null)
        {
            //
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
                if (Parent == null)
                    return 0;
                else
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
        public ReadOnlyCollection<ITreeItemAdv> ChildCollection
        {
            get { return new ReadOnlyCollection<ITreeItemAdv>(_children); }
        }


        /// <summary>
        /// Gets a recursive list of this item plus all the children.
        /// </summary>
        /// <value>The recursive list.</value>
        public ReadOnlyCollection<ITreeItemAdv> RecursiveChildCollection
        {
            get
            {
                IList<ITreeItemAdv> list = new List<ITreeItemAdv>();
                list.Add(this);
                foreach (ITreeItemAdv item in ChildCollection)
                {
                    IList<ITreeItemAdv> subList = item.RecursiveChildCollection;
                    foreach (ITreeItemAdv subItem in subList)
                    {
                        list.Add(subItem);
                    }
                }
                return new ReadOnlyCollection<ITreeItemAdv>(list);
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
        public ReadOnlyCollection<ITreeItemAdv> RecursiveParentCollection
        {
            get
            {
                IList<ITreeItemAdv> list = new List<ITreeItemAdv>();
                if (Parent != null)
                {
                    foreach (ITreeItemAdv item in Parent.RecursiveParentCollection)
                    {
                        list.Add(item);
                    }
                    list.Add(Parent);
                }
                return new ReadOnlyCollection<ITreeItemAdv>(list);

            }
        }

        /// <summary>
        /// Gets or sets the tree rootNode.
        /// </summary>
        /// <value>The rootNode.</value>
        public TreeNodeAdv Node
        {
            get { return _node; }
            set { _node = value; }
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
            get { return _data; }
            set { _data = value; }
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
            if (_imageIndex != -1)
                return _imageIndex;
            else
            {
                if (!HasParent)
                    return 0;
                else if (!IsLeaf)
                    return 1;
                else
                    return 2;
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ITreeItemAdv Parent
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
        private void setParent(ITreeItemAdv parent)
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
        public ITreeItemAdv InsertChild(int index, ITreeItemAdv item)
        {
            if (item != null)
            {
                TreeItemAdvEventArgs args = OnAdding(item);
                if (!args.Cancel)
                {
                    _children.Insert(index, item);
                    if (!item.HasParent)
                        item.Parent = this;
                    OnAdded(item);
                    return item;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItemAdv InsertChild(int index, string text)
        {
            ITreeItemAdv item = new TreeItemAdv(text);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Inserts a child item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItemAdv InsertChild(int index, string key, string text)
        {
            ITreeItemAdv item = new TreeItemAdv(key, text);
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
        public ITreeItemAdv InsertChild(int index, TreeNodeAdv node, string key, string text)
        {
            ITreeItemAdv item = new TreeItemAdv(node, key, text);
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
        public ITreeItemAdv InsertChild(int index, TreeNodeAdv node, string key, string text, string description, object data)
        {
            ITreeItemAdv item = new TreeItemAdv(node, key, text, description, data);
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
        public ITreeItemAdv InsertChild(int index, string key, string text, string description, object data)
        {
            ITreeItemAdv item = new TreeItemAdv(key, text, description, data);
            return InsertChild(index, item);
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public ITreeItemAdv AddChild(ITreeItemAdv item)
        {
            if (item != null && FindItem(item, SearchRangeOption.Children) == null)
            {
                TreeItemAdvEventArgs args = OnAdding(item);
                if (!args.Cancel)
                {
                    _children.Add(item);
                    if (!item.HasParent)
                        item.Parent = this;
                    OnAdded(item);
                    return item;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItemAdv AddChild(string text)
        {
            ITreeItemAdv item = new TreeItemAdv(this, text);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItemAdv AddChild(string key, string text)
        {
            ITreeItemAdv item = new TreeItemAdv(this, key, text);
            return item;
        }

        /// <summary>
        /// Adds a child item.
        /// </summary>
        /// <param name="node">The Node.</param>
        /// <param name="key">The key.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITreeItemAdv AddChild(TreeNodeAdv node, string key, string text)
        {
            ITreeItemAdv item = new TreeItemAdv(this, node, key, text);
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
        public ITreeItemAdv AddChild(TreeNodeAdv node, string key, string text, string description, object data)
        {
            ITreeItemAdv item = new TreeItemAdv(this, node, key, text, description, data);
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
        public ITreeItemAdv AddChild(string key, string text, string tag, object data)
        {
            ITreeItemAdv item = new TreeItemAdv(this, null, key, text, tag, data);
            return item;
        }

        #region ITreeItemAdv Members

        
        #endregion

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="range">The search range.</param>
        /// <returns></returns>
        public ITreeItemAdv FindItem(string key, SearchRangeOption range)
        {
            if (range == SearchRangeOption.ThisAndChildren || range == SearchRangeOption.ThisAndRecursiveChildren || range == SearchRangeOption.ThisOnly)
            {
                if (Key == key)
                    return this;
            }
            if (range != SearchRangeOption.ThisOnly)
            {
                foreach (ITreeItemAdv item in ChildCollection)
                {
                    if (range == SearchRangeOption.Children || range == SearchRangeOption.ThisAndChildren)
                    {
                        ITreeItemAdv result = item.FindItem(key, SearchRangeOption.ThisOnly);
                        if (result != null)
                            return result;
                    }
                    else
                    {
                        ITreeItemAdv result = item.FindItem(key, SearchRangeOption.ThisAndRecursiveChildren);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public ITreeItemAdv FindItem(TreeNodeAdv node, SearchRangeOption range)
        {
            if (node == null)
                return null;
            if (range == SearchRangeOption.ThisAndChildren || range == SearchRangeOption.ThisAndRecursiveChildren || range == SearchRangeOption.ThisOnly)
            {
                if (Node == node)
                    return this;
            }
            if (range != SearchRangeOption.ThisOnly)
            {
                foreach (ITreeItemAdv item in ChildCollection)
                {
                    if (range == SearchRangeOption.Children || range == SearchRangeOption.ThisAndChildren)
                    {
                        ITreeItemAdv result = item.FindItem(node, SearchRangeOption.ThisOnly);
                        if (result != null)
                            return result;
                    }
                    else
                    {
                        ITreeItemAdv result = item.FindItem(node, SearchRangeOption.ThisAndRecursiveChildren);
                        if (result != null)
                            return result;
                    }
                }
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
        public ITreeItemAdv FindItem(object data, SearchRangeOption range)
        {
            if (data == null)
                return null;
            if (range == SearchRangeOption.ThisAndChildren || range == SearchRangeOption.ThisAndRecursiveChildren || range == SearchRangeOption.ThisOnly)
            {
                if (data == Data)
                    return this;
            }
            if (range != SearchRangeOption.ThisOnly)
            {
                foreach (ITreeItemAdv item in ChildCollection)
                {
                    if (range == SearchRangeOption.Children || range == SearchRangeOption.ThisAndChildren)
                    {
                        ITreeItemAdv result = item.FindItem(data, SearchRangeOption.ThisOnly);
                        if (result != null)
                            return result;
                    }
                    else
                    {
                        ITreeItemAdv result = item.FindItem(data, SearchRangeOption.ThisAndRecursiveChildren);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a child item.
        /// </summary>
        /// <param name="treeItem">The item.</param>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public ITreeItemAdv FindItem(ITreeItemAdv treeItem, SearchRangeOption range)
        {
            if (treeItem == null)
                return null;
            if (range == SearchRangeOption.ThisAndChildren || range == SearchRangeOption.ThisAndRecursiveChildren || range == SearchRangeOption.ThisOnly)
            {
                if (this == treeItem)
                    return this;
            }
            if (range != SearchRangeOption.ThisOnly)
            {
                foreach (ITreeItemAdv item in ChildCollection)
                {
                    if (range == SearchRangeOption.Children || range == SearchRangeOption.ThisAndChildren)
                    {
                        ITreeItemAdv result = item.FindItem(treeItem, SearchRangeOption.ThisOnly);
                        if (result != null)
                            return result;
                    }
                    else
                    {
                        ITreeItemAdv result = item.FindItem(treeItem, range);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Remove()
        {
            if (Parent != null)
                return Parent.RemoveChild(this);
            else
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
            SearchRangeOption range = SearchRangeOption.ThisOnly;
            if (recursiveSearch)
                range = SearchRangeOption.ThisAndRecursiveChildren;
            foreach (ITreeItemAdv child in this.ChildCollection)
            {
                ITreeItemAdv item = child.FindItem(key, range);
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
        public bool RemoveChild(TreeNodeAdv node, bool recursiveSearch)
        {
            SearchRangeOption range = SearchRangeOption.ThisOnly;
            if (recursiveSearch)
                range = SearchRangeOption.ThisAndRecursiveChildren;
            foreach (ITreeItemAdv child in ChildCollection)
            {
                ITreeItemAdv item = child.FindItem(node, range);
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
        public bool RemoveChild(ITreeItemAdv item)
        {
            if (item == null)
                return false;
            TreeItemAdvEventArgs args = OnRemoving(item);
            if (args.Cancel == false)
            {
                if (_children.Remove(item))
                {
                    OnRemoved(item);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        ///// <summary>
        ///// Take data from another ITreeItemAdv.
        ///// </summary>
        ///// <param name="master">The master item to take the data from.</param>
        ///// <remarks>
        ///// It only gets the data part from the master, but not the visualization part.
        ///// </remarks>
        ////public void JoinWith(ITreeItemAdv master)
        ////{
        ////    throw new NotImplementedException();
        ////}

        /// <summary>
        /// Gets or sets the data that is connected with the visualization.
        /// </summary>
        /// <value>The node display data.</value>
        public object NodeDisplayInfo
        {
            get
            {
                return _nodeDisplayData;
            }
            set
            {
                _nodeDisplayData = value;
            }
        }

        /// <summary>
        /// Saves the node display info.
        /// </summary>
        public void SaveNodeDisplayInfo()
        {
            //foreach (ITreeItemAdv item in RecursiveChildCollection)
            //{
            //    if (item.Node != null)
            //        item.NodeDisplayInfo = new TreeNodeAdvDisplayInfo(item);
            //}
        }

        #endregion

        #region OnEvent methods

        /// <summary>
        /// Fires the Removing event.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected virtual TreeItemAdvEventArgs OnRemoving(ITreeItemAdv item)
        {
            TreeItemAdvEventArgs args = new TreeItemAdvEventArgs(item, _parent);
            //TODO:Commented by Dinesh
            if (Removing != null)
                Removing(this, args);
            return args;
        }

        /// <summary>
        /// Fires the Adding event.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected virtual TreeItemAdvEventArgs OnAdding(ITreeItemAdv item)
        {
            TreeItemAdvEventArgs args = new TreeItemAdvEventArgs(item, this);
            //TODO:Commented by Dinesh
            if (Adding != null)
                Adding(this, args);
            return args;
        }

        /// <summary>
        /// Fires the Removed event.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnRemoved(ITreeItemAdv item)
        {
            TreeItemAdvEventArgs args = new TreeItemAdvEventArgs(item, _parent);
            //TODO:Commented by Dinesh
            if (Removed != null)
                Removed(this, args);
        }

        /// <summary>
        /// Fires the Added event.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnAdded(ITreeItemAdv item)
        {
            TreeItemAdvEventArgs args = new TreeItemAdvEventArgs(item, this);
            //TODO:Commented by Dinesh
            if (Added != null)
                Added(this, args);
        }

        #endregion

        
    }         
    
}
