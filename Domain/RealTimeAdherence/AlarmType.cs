using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
	public class AlarmType : VersionedAggregateRootWithBusinessUnit, IAlarmType, IDeleteTag
    {
        private Description _description;
        private Color _displayColor;
        private TimeSpan _thresholdTime;
        private double _staffingEffect;
        private bool _isDeleted;
		private Adherence? _adherence;

		public AlarmType(Description description, Color color, TimeSpan thresholdTime, double staffingEffect)
        {
            _description = description;
            _displayColor = color;
            _thresholdTime = thresholdTime;
            _staffingEffect = staffingEffect;
        }

		public AlarmType()
        {}

        public virtual double StaffingEffect
        {
            get { return _staffingEffect; }
            set{ _staffingEffect = value;}
        }

		public virtual Adherence? Adherence
		{
			get { return _adherence; }
			set { _adherence = value; }
		}

		private static readonly IEnumerable<adherenceWithText> _adherences = new[]
		{
			new adherenceWithText
			{
				Adherence = Interfaces.Domain.Adherence.In,
				Text = Resources.InAdherence
			},
			new adherenceWithText
			{
				Adherence = Interfaces.Domain.Adherence.Out,
				Text = Resources.OutOfAdherence
			},
			new adherenceWithText
			{
				Adherence = Interfaces.Domain.Adherence.Neutral,
				Text = Resources.NeutralAdherence
			}
		};

		private class adherenceWithText
		{
			public Adherence Adherence { get; set; }
			public string Text { get; set; }
		}

		public virtual void SetAdherenceByText(string text)
		{
			Adherence = _adherences.Single(x => x.Text == text).Adherence;
		}

		public virtual string AdherenceText
		{
			get
			{
				if (Adherence == null)
					return string.Empty;
				return _adherences.Single(x => x.Adherence == Adherence).Text;
			}
		}

		public virtual TimeSpan ThresholdTime
        {
            get { return _thresholdTime; }
            set
            {
                if (value<TimeSpan.Zero) throw new ArgumentOutOfRangeException("value", "A negative threshold time cannot be used for alarm");
                _thresholdTime = value;
            }
        }

        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set { _displayColor = value; }
        }

		public virtual Description ConfidentialDescription(IPerson assignedPerson, DateOnly assignedDate)
        {
            return Description;
        }

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson, DateOnly assignedDate)
        {
            return DisplayColor;
        }

        public virtual bool InContractTime { get; set; }
        public virtual ITracker Tracker { get; set; }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual IPayload UnderlyingPayload
        {
            get { return this; }
        }

    }

}