using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{
    /// <summary>
    /// Represents the RuleSetRenameEventArgs
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// </remarks>
    public class RuleSetRenameEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the name of the rule set.
        /// </summary>
        /// <value>The name of the rule set.</value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public string RuleSetName 
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSetRenameEventArgs"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public RuleSetRenameEventArgs(string name) : base()
        {
            RuleSetName = name;
        }
    }

    /// <summary>
    /// Data source for TemplateView Grid in Shift Creator
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 2008-05-13
    /// </remarks>
    public class GeneralTemplateView : EntityContainer<WorkShiftRuleSet>, IGridDataValidator
    {
        #region Variables

        private readonly WorkShiftRuleSet _ruleSet;
        private readonly ContractTimeLimiter _workLimiter;
        private string _activity;
        private TimeSpan _startEarly;
        private TimeSpan _startLate;
        private TimeSpan _startSegment;
        private TimeSpan _lateEarly;
        private TimeSpan _lateLate;
        private TimeSpan _lateSegment;
        private TimeSpan _workingStartTime;
        private TimeSpan _workingEndTime;
        private TimeSpan _workingSegment;

        private int _defaultSegment;

        private IList<IActivity> _activities;

        public event EventHandler<RuleSetRenameEventArgs> NotifyParent;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralTemplateView"/> class.
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        /// <param name="activities">The activities.</param>
        /// <param name="defaultSegment">The default segment.</param>
        public GeneralTemplateView(WorkShiftRuleSet ruleSet, IList<IActivity> activities, int defaultSegment)
        {
            _ruleSet = ruleSet;
            _activities = activities;

            //_defaultSegment = (int)(StateHolder.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.DefaultSegment]);
            _defaultSegment = defaultSegment;

            IEnumerable<ContractTimeLimiter> limiters = ruleSet.LimiterCollection.OfType<ContractTimeLimiter>();

            if (limiters.Count()==0)
            {
                ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), 
                                                           new TimeSpan(8, 0, 0)),
                                                           TimeSpan.FromMinutes(_defaultSegment)));
            }
            _workLimiter = limiters.First();

            this._startEarly = _ruleSet.TemplateGenerator.StartPeriod.Period.StartTime;
            this._startLate = _ruleSet.TemplateGenerator.StartPeriod.Period.EndTime;
            this._startSegment = _ruleSet.TemplateGenerator.StartPeriod.Segment;

            this._lateEarly = _ruleSet.TemplateGenerator.EndPeriod.Period.StartTime;
            this._lateLate = _ruleSet.TemplateGenerator.EndPeriod.Period.EndTime;
            this._lateSegment = _ruleSet.TemplateGenerator.EndPeriod.Segment;

            this._workingStartTime = _workLimiter.TimeLimit.StartTime;
            this._workingEndTime = _workLimiter.TimeLimit.EndTime;
            this._workingSegment = _workLimiter.LengthSegment;

            _activity = _ruleSet.TemplateGenerator.BaseActivity.Description.Name;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the rule set id.
        /// </summary>
        /// <value>The rule set id.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/3/2008
        /// </remarks>
        public Guid? RuleSetId
        {
            get
            {
                return _ruleSet.Id;
            }
        }

        /// <summary>
        /// Gets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/24/2008
        /// </remarks>
        public string RuleSet
        {
            get
            {
                return _ruleSet.Description.Name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _ruleSet.Description = new Description(value, string.Empty);
                    if(NotifyParent != null)
                        NotifyParent(this, new RuleSetRenameEventArgs(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the base activity.
        /// </summary>
        /// <value>The base activity.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public string BaseActivity
        {
            get
            {
                //return _ruleSet.TemplateGenerator.BaseActivity;
                return _activity;
            }
            set
            {
                //InParameter.NotNull("BaseActivity", _ruleSet.TemplateGenerator.BaseActivity);
                //InParameter.NotNull("BaseActivity", _activity);
                //_ruleSet.TemplateGenerator.BaseActivity = value;
                _activity = value;
            }
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public IShiftCategory Category
        {
            get
            {
                return _ruleSet.TemplateGenerator.Category;
            }
            set
            {
                InParameter.NotNull("Category", _ruleSet.TemplateGenerator.Category);
                _ruleSet.TemplateGenerator.Category = value;
            }
        }

        /// <summary>
        /// Gets or sets the accessibility.
        /// </summary>
        /// <value>The accessibility.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-23
        /// </remarks>
        public string Accessibility
        {
            get { return Enum.GetName(typeof(DefaultAccessibility), _ruleSet.DefaultAccessibility); }
            set { _ruleSet.DefaultAccessibility = (DefaultAccessibility)Enum.Parse(typeof(DefaultAccessibility), value, true); }
        }

        /// <summary>
        /// Gets or sets the start period start time.
        /// </summary>
        /// <value>The start period start time.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan StartPeriodStartTime
        {
            get
            {
                return this._startEarly;
            }
            set
            {
                this._startEarly = value;
            }
        }

        /// <summary>
        /// Gets or sets the start period end time.
        /// </summary>
        /// <value>The start period end time.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan StartPeriodEndTime
        {
            get
            {
                return this._startLate;
            }
            set
            {
                this._startLate = value;
            }
        }

        /// <summary>
        /// Gets or sets the start period segment.
        /// </summary>
        /// <value>The start period segment.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan StartPeriodSegment
        {
            get
            {
                return this._startSegment;
            }
            set
            {
                this._startSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets the end period start time.
        /// </summary>
        /// <value>The end period start time.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan EndPeriodStartTime
        {
            get
            {
                return this._lateEarly;
            }
            set
            {
                this._lateEarly = value;
            }
        }

        /// <summary>
        /// Gets or sets the end period end time.
        /// </summary>
        /// <value>The end period end time.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan EndPeriodEndTime
        {
            get
            {
                return this._lateLate;
            }
            set
            {
                this._lateLate = value;
            }
        }

        /// <summary>
        /// Gets or sets the start period segment.
        /// </summary>
        /// <value>The start period segment.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public TimeSpan EndPeriodSegment
        {
            get
            {
                return this._lateSegment;
            }
            set
            {
                this._lateSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets the length segment.
        /// </summary>
        /// <value>The length segment.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 22-08-2008
        /// </remarks>
        public TimeSpan WorkingSegment
        {
            get
            {
                return this._workingSegment;
            }
            set
            {
                this._workingSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 12-06-2008
        /// </remarks>
        public TimeSpan WorkingStartTime
        {
            get
            {
                return this._workingStartTime;
            }
            set
            {
                this._workingStartTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the working end time.
        /// </summary>
        /// <value>The end time.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 12-06-2008
        /// </remarks>
        public TimeSpan WorkingEndTime
        {
            get
            {
                return _workingEndTime;
            }
            set
            {
                this._workingEndTime = value;
            }
        }


        #endregion

        #region IGridDataValidator Members

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-23
        /// </remarks>
        public bool Validate()
        {
            bool status = true;
            bool startStatus = true;
            bool endStatus = true;
            bool workingStatus = true;

            //if(_startEarly.Equals(TimeSpan.Zero))
              //  startStatus = false;
            //if (_startLate.Equals(TimeSpan.Zero))
            //    startStatus = false;
            if((startStatus) && (_startEarly > _startLate))
                startStatus = false;
            if ((startStatus) && (_startSegment.Equals(TimeSpan.Zero)))
                startStatus = false;

            if((_lateEarly > _lateLate) && (!_lateLate.Equals(TimeSpan.Zero)))
                endStatus = false;
            if ((endStatus) && (_lateSegment.Equals(TimeSpan.Zero)))
                endStatus = false;

            if (_workingStartTime > _workingEndTime)
                workingStatus = false;
            if ((workingStatus) && (_workingSegment.Equals(TimeSpan.Zero)))
                endStatus = false;

            status = (startStatus && endStatus && workingStatus);
            if(status)
            {
                if(_lateEarly < _startEarly)
                {
                    _lateEarly = new TimeSpan(1, _lateEarly.Hours, _lateEarly.Minutes, _lateEarly.Seconds);
                    if(_lateLate.Equals((TimeSpan.Zero)))
                        _lateLate = new TimeSpan((_lateEarly.Days + 1), _lateLate.Hours, _lateLate.Minutes, _lateLate.Seconds);
                    if(_lateLate < _lateEarly)
                        _lateLate = new TimeSpan(_lateEarly.Days, _lateLate.Hours, _lateLate.Minutes, _lateLate.Seconds);
                }
                else if(_lateEarly > _lateLate)
                {
                    if (_lateLate.Equals((TimeSpan.Zero)))
                        _lateLate = new TimeSpan((_lateEarly.Days + 1), _lateLate.Hours, _lateLate.Minutes, _lateLate.Seconds);
                    if (_lateLate < _lateEarly)
                        _lateLate = new TimeSpan(_lateEarly.Days, _lateLate.Hours, _lateLate.Minutes, _lateLate.Seconds);
                }

                TimePeriodWithSegment startTimePeriod = new TimePeriodWithSegment(new TimePeriod(_startEarly, _startLate), _startSegment);
                TimePeriodWithSegment endTimePeriod = new TimePeriodWithSegment(new TimePeriod(_lateEarly, _lateLate), _lateSegment);
                _ruleSet.TemplateGenerator.StartPeriod = startTimePeriod;
                _ruleSet.TemplateGenerator.EndPeriod = endTimePeriod;

                _workLimiter.TimeLimit = new TimePeriod(_workingStartTime, _workingEndTime);
                _workLimiter.LengthSegment = _workingSegment;
            }

            _ruleSet.TemplateGenerator.BaseActivity = (from p in _activities where p.Name.Equals(_activity) select p).First();

            return status;
        }

        #endregion
    }

}
