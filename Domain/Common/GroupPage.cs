﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a Grouping Unit.
    /// </summary>
    public class GroupPage : AggregateRootWithBusinessUnit, IDeleteTag, IGroupPage
    {
        private Description _description;
        private readonly IList<IRootPersonGroup> _rootGroupCollection = new List<IRootPersonGroup>();
        private bool _isDeleted;
        private string _rootNodeName;
        private string _descriptionKey;
    	private bool _isUserDefined;

    	protected GroupPage()
        {}

        public GroupPage(string description) : this()
        {
            InParameter.NotStringEmptyOrNull("description", description);
            _description = new Description(description);
        }

        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        
        public virtual string DescriptionKey
        {
            get
            {
				checkKey();
                return _descriptionKey;
            }
            set { _descriptionKey = value; }
        }

        public virtual string Key
        {
            get
            {
                return _descriptionKey ?? _description.Name;
            }
        }

		private void checkKey()
		{
			if (_descriptionKey == null && Id.HasValue)
			{
				_descriptionKey = Id.Value.ToString();
				_isUserDefined = true;
			}
		}
    	public virtual bool IsUserDefined()
    	{
    		checkKey();
    		return _isUserDefined;
    	}

    	public virtual ReadOnlyCollection<IRootPersonGroup> RootGroupCollection
        {
            get
            {
            	return
            		new ReadOnlyCollection<IRootPersonGroup>(_rootGroupCollection.OrderBy(r => r.Description.Name).ToList());
            }
        }

        public virtual string RootNodeName
        {
            get { return _rootNodeName ?? _description.Name; }
            set { _rootNodeName = value; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void AddRootPersonGroup(IRootPersonGroup group)
        {
            InParameter.NotNull("group", group);

            if (!_rootGroupCollection.Contains(group))
            {
                group.SetParent(this);
                _rootGroupCollection.Add(group);
            }
        }

        public virtual void RemoveRootPersonGroup(IRootPersonGroup group)
        {
            InParameter.NotNull("group", group);

            if (_rootGroupCollection.Contains(group))
            {
                _rootGroupCollection.Remove(group);
            }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

		public override int GetHashCode()
		{
			var idHash = 0;
			if (Id.HasValue)
				idHash = Id.GetHashCode();

			var descrHash = 0;
			if (DescriptionKey != null)
				descrHash = DescriptionKey.GetHashCode();

			return idHash ^ descrHash;
		}

		public override bool Equals(object obj)
		{
			var ent = obj as IGroupPage;
			if (ent == null)
				return false;
			return Equals(ent);
		}

		public virtual bool Equals(IGroupPage other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return (GetHashCode() == other.GetHashCode());
		}
    }
}