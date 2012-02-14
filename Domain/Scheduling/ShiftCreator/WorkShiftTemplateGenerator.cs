using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Createa a list of Shifts to use on all the Definitions 
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-03-06    
    /// </remarks>
    public class WorkShiftTemplateGenerator : IWorkShiftTemplateGenerator
    {

		#region Fields (4) 

        private IActivity _baseActivity;
        private IShiftCategory _category;
        private TimePeriodWithSegment _endPeriod;
        private TimePeriodWithSegment _startPeriod;

		#endregion Fields 

        public WorkShiftTemplateGenerator(IActivity activity, 
                                        TimePeriodWithSegment startPeriod, 
                                        TimePeriodWithSegment endPeriod,
                                        IShiftCategory category)
        {
            InParameter.NotNull("activity", activity);
            InParameter.NotNull("category", category);
            _startPeriod = startPeriod;
            _endPeriod = endPeriod;
            _baseActivity = activity;
            _category = category;
        }

        protected WorkShiftTemplateGenerator(){}

		#region Properties (4) 

        /// <summary>
        /// Gets or sets the base activity.
        /// </summary>
        /// <value>The base activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual IActivity BaseActivity
        {
            get { return _baseActivity; }
            set
            {
                InParameter.NotNull("value", value);
                _baseActivity = value;
            }
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-10
        /// </remarks>
        public virtual IShiftCategory Category
        {
            get { return _category; }
            set
            {
                InParameter.NotNull("value", value);
                _category = value;
            }
        }

        /// <summary>
        /// Gets or sets the possible end period.
        /// </summary>
        /// <value>The end period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual TimePeriodWithSegment EndPeriod
        {
            get { return _endPeriod; }
            set { _endPeriod = value; }
        }

        /// <summary>
        /// Gets or sets the possible start period.
        /// </summary>
        /// <value>The start period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual TimePeriodWithSegment StartPeriod
        {
            get { return _startPeriod; }
            set { _startPeriod=value; }
        }

		#endregion Properties 

		#region Methods (1) 


		// Public Methods (1) 

        /// <summary>
        /// Generates a List of Work Shifts.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-03-06
        /// /// </remarks>
        public virtual IList<IWorkShift> Generate()
        {
            IList<IWorkShift> retCol = new List<IWorkShift>();
            for (TimeSpan actStart = _startPeriod.Period.StartTime;
                 actStart <= _startPeriod.Period.EndTime;
                 actStart = actStart.Add(_startPeriod.Segment))
            {
                for (TimeSpan actEnd = _endPeriod.Period.StartTime;
                     actEnd <= _endPeriod.Period.EndTime;
                     actEnd = actEnd.Add(_endPeriod.Segment))
                {
                    if(actEnd>actStart)
                    {
                        IWorkShift newShift = new WorkShift(Category);
                        DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(actStart), WorkShift.BaseDate.Add(actEnd));
                        newShift.LayerCollection.Add(new WorkShiftActivityLayer(BaseActivity, period));
                        retCol.Add(newShift);                        
                    }
                }

            }
            return retCol;
        }


		#endregion Methods 

    
        #region ICloneableEntity<WorkShiftTemplateGenerator> Members

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public IWorkShiftTemplateGenerator NoneEntityClone()
        {
            return (WorkShiftTemplateGenerator)MemberwiseClone();
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public IWorkShiftTemplateGenerator EntityClone()
        {
            return NoneEntityClone();
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public object Clone()
        {
            return EntityClone();
        }

        #endregion
    }
}
