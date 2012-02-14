using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using System.Drawing;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Represents an activity (something to do)
    /// </summary>
    [Serializable]
    public class Activity : Payload, IActivity
    {
        private Description _description;
        private Color _displayColor;
        private bool _requiresSkill;
        private bool _inReadyTime;
        private string _payrollCode;
        private IGroupingActivity _groupingActivity;
        
        private ReportLevelDetail _reportLevelDetail;
    	private bool _requiresSeat;

        [NonSerialized]
        private readonly DeletedDescription _deletedDescription = new DeletedDescription();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Activity(string name)
            : base(true)
        {
            Description = new Description(name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        protected Activity()
        {
        }

        #endregion

		public virtual Description Description
		{
            get
            {
                if (IsDeleted)
                {
                    return _deletedDescription.AppendDeleted(_description);
                }
                return _description;
            }
            set { _description = value; }
		}


		public virtual Color DisplayColor
		{
			get { return _displayColor; }
			set { _displayColor = value; }
		}

		public virtual bool InReadyTime
		{
			get { return _inReadyTime; }
			set { _inReadyTime = value; }
		}


		public virtual bool RequiresSkill
		{
			get { return _requiresSkill; }
			set { _requiresSkill = value; }
		}
		public virtual string Name
		{
			get { return Description.Name; }
		}


		/// <summary>
		/// Parent grouping activity
		/// </summary>
		public virtual IGroupingActivity GroupingActivity
		{
			get { return _groupingActivity; }
			set
			{
				InParameter.NotNull("value", value);
				if (_groupingActivity == value) return;
				if (_groupingActivity != null) _groupingActivity.RemoveActivity(this);
				_groupingActivity = value;
				_groupingActivity.AddActivity(this);
			}
		}



		public virtual ReportLevelDetail ReportLevelDetail
		{
			get { return _reportLevelDetail; }
			set { _reportLevelDetail = value; }
		}

		public virtual string PayrollCode
		{
			get{ return _payrollCode; }
			set{ _payrollCode = value; }
		}

    	public virtual bool RequiresSeat
    	{
    		get {
    			return _requiresSeat;
    		}
    		set {
    			_requiresSeat = value;
    		}
    	}

    	public override string ToString()
        {
            return String.Concat(Description.Name, ", ", base.ToString());
        }

		public virtual Description ConfidentialDescription(IPerson assignedPerson)
		{
			return Description;
		}

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson)
		{
			return DisplayColor;
		}

        public override IPayload UnderlyingPayload
        {
            get { return this; }
        }
	}
}
