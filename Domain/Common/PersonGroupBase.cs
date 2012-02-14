﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: kosalanp
    /// Created date: 2008-06-24
    /// </remarks>
    public abstract class PersonGroupBase : AggregateEntity
    {
        private Description _description;
        private readonly IList<IPerson> _personCollection = new List<IPerson>();
        private readonly IList<IChildPersonGroup> _childGroupCollection = new List<IChildPersonGroup>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonGroupBase"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 26/06/2008
        /// </remarks>
        protected PersonGroupBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonGroupBase"/> class.
        /// </summary>
        /// <param name="description">The name to set.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-24
        /// </remarks>
        protected PersonGroupBase(string description)
            : this()
        {
            InParameter.NotStringEmptyOrNull("description", description);
            _description = new Description(description);
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-23
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-24
        /// </remarks>
        public virtual ReadOnlyCollection<IPerson> PersonCollection
        {
            get { return new ReadOnlyCollection<IPerson>(_personCollection); }
        }

        /// <summary>
        /// Gets the child collection.
        /// </summary>
        /// <value>The child collection.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        public virtual ReadOnlyCollection<IChildPersonGroup> ChildGroupCollection
        {
            get { return new ReadOnlyCollection<IChildPersonGroup>(_childGroupCollection); }
        }

        /// <summary>
        /// Adds the person to group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-23
        /// </remarks>
        public virtual void AddPerson(IPerson person)
        {
            InParameter.NotNull("person", person);
            if (!_personCollection.Contains(person))
            {
                _personCollection.Add(person);
            }
        }

        /// <summary>
        /// Removes the person from group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-23
        /// </remarks>
        public virtual void RemovePerson(IPerson person)
        {
            InParameter.NotNull("person", person);
            _personCollection.Remove(person);
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="group">The child.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        public virtual void AddChildGroup(IChildPersonGroup group)
        {
            InParameter.NotNull("group", group);

            if (!_childGroupCollection.Contains(group))
            {
                group.SetParent(this);
                _childGroupCollection.Add(group);
            }
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="group">The child.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        public virtual void RemoveChildGroup(IChildPersonGroup group)
        {
            InParameter.NotNull("group", group);
            _childGroupCollection.Remove(group);
        }

        public virtual IAggregateRoot Entity { get; set; }

        public virtual bool IsTeam { get { return Entity != null && typeof(ITeam).IsInstanceOfType(Entity); } }

        public virtual bool IsSite { get { return Entity != null && typeof(ISite).IsInstanceOfType(Entity); } }
    }
}
