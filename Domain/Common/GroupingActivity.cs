using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    [Serializable]
    public class GroupingActivity : VersionedAggregateRoot, IGroupingActivity, IDeleteTag
    {
        #region Fields

        private Description _description;
        private readonly IList<IActivity> _activityCollection;
        private bool _isDeleted;

        #endregion

        #region Properties

        /// <summary>
        /// Set/Get for description
        /// </summary>     
        public virtual Description Description
        {
            get { return _description; }
            set { _description=value; }
        }

        #endregion

        /// <summary>
        /// Gets the name part of the description.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-26
        /// </remarks>
        public virtual string Name
        {
            get { return _description.Name; }
        }
        /// <summary>
        /// CreateProjection new GroupingActivity
        /// </summary>
        public GroupingActivity(string nameToSet)
            : this()
        {
            _description= new Description(nameToSet);
        }

        /// <summary>
        /// Empty contructor for NHibernate
        /// </summary>
        protected GroupingActivity()
        {
            _activityCollection = new List<IActivity>();
        }

        /// <summary>
        /// Gets the activities.
        /// Read only wrapper around the actual activity list.
        /// </summary>
        /// <value>The activities list.</value>
        public virtual ReadOnlyCollection<IActivity> ActivityCollection
        {
            get { return new ReadOnlyCollection<IActivity>(_activityCollection); }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        /// <summary>
        /// Adds an Activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        public virtual void AddActivity(IActivity activity)
        {
            InParameter.NotNull("activity", activity);
            if (_activityCollection.Contains(activity))
            {
                _activityCollection.Remove(activity);
            }
            _activityCollection.Add(activity);
            activity.GroupingActivity = this;
        }

        /// <summary>
        /// Remove an Activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        public virtual void RemoveActivity(IActivity activity)
        {
            InParameter.NotNull("activity", activity);
            if (_activityCollection.Contains(activity))
            {
                _activityCollection.Remove(activity);
            }
        }


        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}