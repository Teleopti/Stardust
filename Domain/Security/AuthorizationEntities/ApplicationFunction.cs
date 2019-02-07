using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	[DebuggerDisplay("{ForeignId}, {Id.ToString().ToUpper()}, {FunctionPath}")]
	public class ApplicationFunction : AggregateRoot_Events_ChangeInfo_Versioned, IApplicationFunction, IDeleteTag
    {
        private string _functionDescription;
        private IParentChildEntity _parent;
        private readonly IList<IParentChildEntity> _children = new List<IParentChildEntity>();
        private string _functionCode;
        private string _foreignId;
        private string _foreignSource;
        private int? _sortOrder;
        private bool? _isPermitted;
        private bool _isPreliminary;
        private static IUserTextTranslator _userTextTranslator = new UserTextTranslator();
        private bool _isDeleted;
        private string _functionPathCache;

        public ApplicationFunction(string functionCode)
        {
            InParameter.NotStringEmptyOrNull(nameof(functionCode), functionCode);
            _functionCode = functionCode;
            _functionDescription = functionCode;
        }

        public ApplicationFunction(string functionCode, IApplicationFunction parent)
            : this(functionCode)
        {
            InParameter.NotNull(nameof(parent), parent);
            Parent = parent;
        }

        public ApplicationFunction() : this("ApplicationFunction")
        {
        }

        /// <summary>
        /// Removes the application role from the context.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <remarks>
        /// You have to call this function before you delete the application function from repository. 
        /// </remarks>
        public virtual void RemoveApplicationRoleFromContext(IEnumerable<IApplicationRole> applicationRoles)
        {
            foreach (IApplicationRole role in applicationRoles)
            {
                if (role.ApplicationFunctionCollection.Contains(this))
                    role.RemoveApplicationFunction(this);
            }
        }

        /// <summary>
        /// Finds the application function in the list by foreign source and id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="source">The source.</param>
        /// <param name="id">The id.</param>
        /// <returns>If the item is found, then the item, <c>null</c> otherwise.</returns>
        public static IApplicationFunction FindByForeignId(IEnumerable<IApplicationFunction> list, string source, string id)
        {
	        var lookup = list.Where(l => !string.IsNullOrEmpty(l.ForeignId)).ToLookup(l => l.ForeignId.ToUpperInvariant());
	        var found = lookup[id.ToUpperInvariant()].FirstOrDefault();
	        if (!string.IsNullOrEmpty(found?.ForeignSource) && found.ForeignSource == source)
	        {
		        return found;
	        }

			return null;
        }

        /// <summary>
        /// Finds the application function by its path.
        /// </summary>
        /// <param name="functions">The functions collection.</param>
        /// <param name="functionPath">The function path.</param>
        /// <returns></returns>
        public static IApplicationFunction FindByPath(IEnumerable<IApplicationFunction> functions, string functionPath)
        {

            foreach (IApplicationFunction item in functions)
            {
                if (item.FunctionPath == functionPath)
                {
                    return item;
                }
            }
            return null;
        }
		
        /// <summary>
        /// Finds the code from path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetParentPath(string path)
        {
            int lastSeparator = path.LastIndexOf("/", StringComparison.Ordinal);
            if (lastSeparator == -1)
                return string.Empty;
            return path.Substring(0, lastSeparator);
        }

        /// <summary>
        /// Finds the code from path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetCode(string path)
        {
            int lastSeparator = path.LastIndexOf("/", StringComparison.Ordinal);
            if (lastSeparator == -1)
                return path;
            return path.Substring(lastSeparator + 1);
        }

        /// <summary>
        /// Gets the level in the application function list hierarchy. Top level is 0.
        /// </summary>
        /// <value>The level.</value>
        public virtual int Level
        {
            get
            {
                if (Parent == null)
                    return 0;
                return 1 + Parent.Level;
            }
        }

        /// <summary>
        /// Gets or sets the FunctionDescription value.
        /// </summary>
        /// <value>The FunctionDescription value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public virtual string FunctionDescription
        {
            get {
                if (string.IsNullOrEmpty(_functionDescription))
                    return "xx" + FunctionCode;
                return _functionDescription;
            }
            set => _functionDescription = value;
        }

        /// <summary>
        /// Gets the localized description text.
        /// </summary>
        /// <value>The FunctionDescription value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 17/08/2008
        /// </remarks>
        public virtual string LocalizedFunctionDescription => _userTextTranslator.TranslateText(FunctionDescription);

	    /// <summary>
        /// Gets or sets the FunctionCode value.
        /// </summary>
        /// <value>The FunctionCode value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public virtual string FunctionCode
        {
            get => _functionCode;
		    set
            {
                _functionCode = value;
                _functionPathCache = null;
            }
        }

        /// <summary>
        /// Gets the FunctionPath value.
        /// </summary>
        /// <value>The FunctionPath value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public virtual string FunctionPath
        {
            get
            {
                // Ola bug 13405 in some cases this cashing gets wrong
                //if (string.IsNullOrEmpty(_functionPathCache))
                //{
                    var parent = Parent as IApplicationFunction;
                    if (parent == null)
                    {
                        _functionPathCache = _functionCode;
                    }
                    else
                    {
                        _functionPathCache = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", parent.FunctionPath,
                                                           _functionCode);
                    }
                //}
                return _functionPathCache;
            }
        }

        /// <summary>
        /// Gets or sets the foreign id.
        /// </summary>
        /// <value>The foreign id.</value>
        /// <remarks>
        /// This is the id that the system stores to check if the foreign application function
        /// has been deleted, or changed. It is used together with ForeignSource property
        /// </remarks>
        public virtual string ForeignId
        {
            get => _foreignId;
	        set => _foreignId = value;
        }

        /// <summary>
        /// Gets or sets the foreign source.
        /// </summary>
        /// <value>The foreign source.</value>
        /// <remarks>
        /// Used together with ForeignId property to define a fogeign application function.
        /// </remarks>
        public virtual string ForeignSource
        {
            get => _foreignSource;
	        set => _foreignSource = value;
        }

	    public virtual bool IsWebReport { get; set; }

		/// <summary>
        /// Sets or gets the local user text translator.
        /// </summary>
        /// <value>The user text translator.</value>
        /// <remarks>
        /// Used for test purposes to change the default translator to an explicit one.
        /// </remarks>
        public virtual IUserTextTranslator UserTextTranslator
        {
            get => _userTextTranslator;
	        set => _userTextTranslator = value;
        }

        /// <summary>
        /// Gets or sets the Parent value.
        /// </summary>
        /// <value>The Parent value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public virtual IParentChildEntity Parent
        {
            get => _parent;
	        set 
            { 
                _parent = value;

	            _parent?.AddChild(this);
	            _functionPathCache = null;
            }
        }

        /// <summary>
        /// Gets or sets the Parent value.
        /// </summary>
        /// <value>The Parent value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public virtual ReadOnlyCollection<IParentChildEntity> ChildCollection => new ReadOnlyCollection<IParentChildEntity>(_children);

	    /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        public virtual void AddChild(IParentChildEntity child)
        {
            if (child != null && !ChildCollection.Contains(child))
            {
                _children.Add(child);
                if (child.Parent == null)
                    child.Parent = this;
            }
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        public virtual void RemoveChild(IParentChildEntity child)
        {
            if (child != null && ChildCollection.Contains(child))
            {
                _children.Remove(child);
                child.Parent = null;
            }
        }

        /// <summary>
        /// Gets the sorting order. Used for setting the order of the Aaplication Function in the function lists.
        /// </summary>
        /// <value>The order.</value>
        public virtual int? SortOrder
        {
            get => _sortOrder;
	        set => _sortOrder = value;
        }

        public virtual bool? IsPermitted
        {
            get => _isPermitted;
	        set => _isPermitted = value;
        }

        public virtual bool IsPreliminary
        {
            get => _isPreliminary;
	        set => _isPreliminary = value;
        }

        public virtual bool IsDeleted => _isDeleted;

	    /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-05-12
        /// </remarks>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
           return FunctionPath.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
		{
			return obj is IApplicationFunction ent && Equals(ent);
		}

        public override bool Equals(IEntity other)
		{
			return other is IApplicationFunction ent && Equals(ent);
		}

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public virtual bool Equals(IApplicationFunction other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;

            return FunctionPath == other.FunctionPath;
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
