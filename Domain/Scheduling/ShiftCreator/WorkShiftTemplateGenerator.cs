using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class WorkShiftTemplateGenerator : IWorkShiftTemplateGenerator
    {
	    private IActivity _baseActivity;
        private IShiftCategory _category;
        private TimePeriodWithSegment _endPeriod;
        private TimePeriodWithSegment _startPeriod;

	    public WorkShiftTemplateGenerator(IActivity activity, 
                                        TimePeriodWithSegment startPeriod, 
                                        TimePeriodWithSegment endPeriod,
                                        IShiftCategory category)
        {
            InParameter.NotNull(nameof(activity), activity);
            InParameter.NotNull(nameof(category), category);
            _startPeriod = startPeriod;
            _endPeriod = endPeriod;
            _baseActivity = activity;
            _category = category;
        }

        protected WorkShiftTemplateGenerator(){}
		
        public virtual IActivity BaseActivity
        {
            get { return _baseActivity; }
            set
            {
                InParameter.NotNull(nameof(BaseActivity), value);
                _baseActivity = value;
            }
        }
		
        public virtual IShiftCategory Category
        {
            get { return _category; }
            set
            {
                InParameter.NotNull(nameof(Category), value);
                _category = value;
            }
        }
		
        public virtual TimePeriodWithSegment EndPeriod
        {
            get { return _endPeriod; }
            set { _endPeriod = value; }
        }
		
        public virtual TimePeriodWithSegment StartPeriod
        {
            get { return _startPeriod; }
            set { _startPeriod=value; }
        }
		
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
		
        public IWorkShiftTemplateGenerator NoneEntityClone()
        {
            return (WorkShiftTemplateGenerator)MemberwiseClone();
        }
		
        public IWorkShiftTemplateGenerator EntityClone()
        {
            return NoneEntityClone();
        }
		
        public object Clone()
        {
            return EntityClone();
        }
    }
}
