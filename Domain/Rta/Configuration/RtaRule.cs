using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Rta.Events;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Rta.Configuration
{
	public class RtaRule : VersionedAggregateRootWithBusinessUnit, IRtaRule
	{
		private Description _description;
		private Color _displayColor;
		private Color _alarmColor;
		private int _thresholdTime;
		private double _staffingEffect;
		private Adherence? _adherence;
		private bool _isAlarm = ServiceLocatorForEntity.AppliedAlarm.RuleDefaultAlarm();

		public RtaRule(Description description, Color color, int thresholdTime, double staffingEffect)
		{
			_description = description;
			_displayColor = color;
			_thresholdTime = thresholdTime;
			_staffingEffect = staffingEffect;
			_alarmColor = color;
		}

		public RtaRule()
		{
		}

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			AddEvent(new RtaRuleChangedEvent());
		}

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
				Adherence = InterfaceLegacy.Domain.Adherence.In,
				Text = Resources.InAdherence
			},
			new adherenceWithText
			{
				Adherence = InterfaceLegacy.Domain.Adherence.Out,
				Text = Resources.OutOfAdherence
			},
			new adherenceWithText
			{
				Adherence = InterfaceLegacy.Domain.Adherence.Neutral,
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
			var adherenceWithText = _adherences.SingleOrDefault(x => x.Text == text);
			Adherence = adherenceWithText == null ? (Adherence?) null : adherenceWithText.Adherence;
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

		public virtual bool IsAlarm
		{
			get
			{
				return _isAlarm;
			}
			set
			{
				_isAlarm = value;
				AddEvent(new RtaRuleChangedEvent());
			}
		}

		public virtual Color AlarmColor { get { return _alarmColor; } set { _alarmColor = value; } }

		public virtual int ThresholdTime
		{
			get { return _thresholdTime; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "A negative threshold time cannot be used for alarm");
				if (value > 0)
					IsAlarm = true;
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

		public virtual Description ConfidentialDescription(IPerson assignedPerson)
		{
			return Description;
		}

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson)
		{
			return DisplayColor;
		}

		public virtual bool InContractTime { get; set; }
		public virtual ITracker Tracker { get; set; }

		public virtual IPayload UnderlyingPayload
		{
			get { return this; }
		}
	}

}