using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Base class for activity start definitions.
    /// Used to place an activity on a work shift in shift creator.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-05
    /// </remarks>
    public abstract class ActivityExtender : AggregateEntity, IWorkShiftExtender
    {

		#region Fields (2) 

        private TimePeriodWithSegment _activityLengthWithSegment;
        private IActivity _extendWithActivity;

		#endregion Fields 

		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityExtender"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="activityLengthWithSegment">Length of the activity.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        protected ActivityExtender(IActivity activity,
                                   TimePeriodWithSegment activityLengthWithSegment)
        {
            InParameter.NotNull(nameof(activity), activity);
            _extendWithActivity = activity;
            _activityLengthWithSegment = activityLengthWithSegment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityExtender"/> class.
        /// Used by nhib.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        protected ActivityExtender(){}

		#endregion Constructors 

		#region Properties (2) 

        /// <summary>
        /// Gets or sets the activity length with segment.
        /// </summary>
        /// <value>The activity length with segment.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimePeriodWithSegment ActivityLengthWithSegment
        {
            get { return _activityLengthWithSegment; }
            set { _activityLengthWithSegment = value; }
        }


        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value> 
        /// <remarks>
        /// The setter is just an empty method.
        /// Must be there right now. Can be removed when nhibernate
        /// supports this out-of-the-box
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        protected virtual int OrderIndex
        {
            get
            {
                if (Parent == null)
                {
                    throw new NullReferenceException("Parent may not be null when calling OrderIndex");
                }

                int ret = ((IWorkShiftRuleSet)Parent).ExtenderCollection.IndexOf(this);
                return ret;
            }
        }

        /// <summary>
        /// Gets the activity to extend.
        /// </summary>
        /// <value>The extend with activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        public virtual IActivity ExtendWithActivity
        {
            get
            {
                return _extendWithActivity;
            }
            set
            {
                InParameter.NotNull(nameof(ExtendWithActivity), value);
                _extendWithActivity = value;
            }
        }

		#endregion Properties 

		#region Methods (2) 


		// Public Methods (2) 

        /// <summary>
        /// Gets the maximum, possible length of the activity.
        /// </summary>
        /// <returns></returns>
        /// <value>The length of the activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        public abstract TimeSpan ExtendMaximum();

        /// <summary>
        /// Replaces the template with new shifts.
        /// </summary>
        /// <param name="shift">The template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        public abstract IList<IWorkShift> ReplaceWithNewShifts(IWorkShift shift);

        #endregion Methods 


        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual IWorkShiftExtender NoneEntityClone()
        {
            ActivityExtender retObj = (ActivityExtender)MemberwiseClone();
            retObj.SetId(null);
            retObj.SetParent(null);
            return retObj;
        }

        public virtual IWorkShiftExtender EntityClone()
        {
            ActivityExtender retObj = (ActivityExtender)MemberwiseClone();
            return retObj;
        }
    }
}
