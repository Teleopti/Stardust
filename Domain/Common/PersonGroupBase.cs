﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public abstract class PersonGroupBase : AggregateEntity
    {
        private Description _description;
        private readonly ISet<IPerson> _personCollection = new HashSet<IPerson>();
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
            InParameter.NotStringEmptyOrNull(nameof(description), description);
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
        public virtual ReadOnlyCollection<IPerson> PersonCollection => new ReadOnlyCollection<IPerson>(_personCollection.ToArray());

	    /// <summary>
        /// Gets the child collection.
        /// </summary>
        /// <value>The child collection.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        public virtual ReadOnlyCollection<IChildPersonGroup> ChildGroupCollection => new ReadOnlyCollection<IChildPersonGroup>(_childGroupCollection);

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
            InParameter.NotNull(nameof(person),person);
            _personCollection.Add(person);
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
            InParameter.NotNull(nameof(person), person);
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
            InParameter.NotNull(nameof(group), group);

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
            InParameter.NotNull(nameof(group), group);
            _childGroupCollection.Remove(group);
        }

        public virtual IAggregateRoot Entity { get; set; }

        public virtual bool IsTeam => Entity != null && typeof(ITeam).IsInstanceOfType(Entity);

	    public virtual bool IsSite => Entity != null && typeof(ISite).IsInstanceOfType(Entity);

		public virtual bool IsOptionalColumn => Entity != null && typeof(IOptionalColumn).IsInstanceOfType(Entity);
	}
}
