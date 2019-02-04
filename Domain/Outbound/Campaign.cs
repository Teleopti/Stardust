using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class Campaign : AggregateRoot_Events_ChangeInfo_BusinessUnit, IOutboundCampaign
	{
		private string _name;
		private ISkill _skill;
		private int _callListLen;
		private int _targetRate;
		private int _connectRate;
		private int _rightPartyConnectRate;
		private int _connectAverageHandlingTime;
		private int _rightPartyAverageHandlingTime;
		private int _unproductiveTime;
		private IDictionary<DateOnly, TimeSpan> _manualProductionPlanDays = new Dictionary<DateOnly, TimeSpan>();
		private IDictionary<DateOnly, TimeSpan> _actualBacklogDays = new Dictionary<DateOnly, TimeSpan>();
		private bool _isDeleted;
		private IDictionary<DayOfWeek, TimePeriod> _workingHours = new Dictionary<DayOfWeek, TimePeriod>();
		private DateTimePeriod _spanningPeriod;
		private DateOnlyPeriod _belongsToPeriod;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual ISkill Skill
		{
			get { return _skill; }
			set { _skill = value; }
		}

		public virtual int CallListLen
		{
			get { return _callListLen; }
			set { _callListLen = value; }
		}

		public virtual int TargetRate
		{
			get { return _targetRate; }
			set { _targetRate = value; }
		}

		public virtual int ConnectRate
		{
			get { return _connectRate; }
			set { _connectRate = value; }
		}

		public virtual int RightPartyConnectRate
		{
			get { return _rightPartyConnectRate; }
			set { _rightPartyConnectRate = value; }
		}


		public virtual int ConnectAverageHandlingTime
		{
			get { return _connectAverageHandlingTime; }
			set { _connectAverageHandlingTime = value; }
		}


		public virtual int RightPartyAverageHandlingTime
		{
			get { return _rightPartyAverageHandlingTime; }
			set { _rightPartyAverageHandlingTime = value; }
		}

		public virtual int UnproductiveTime
		{
			get { return _unproductiveTime; }
			set { _unproductiveTime = value; }
		}

		public virtual IDictionary<DayOfWeek, TimePeriod> WorkingHours
		{
			get { return _workingHours; }
			set { _workingHours = value; }
		}

		public virtual DateTimePeriod SpanningPeriod
		{
			get { return _spanningPeriod; }
			set { _spanningPeriod = value; }
		}

		public virtual DateOnlyPeriod BelongsToPeriod
		{
			get { return _belongsToPeriod; }
			set { _belongsToPeriod = value; }
		}

		public virtual int CampaignTasks()
		{
			if (RightPartyConnectRate == 0)
				return 0;

			return CallListLen*TargetRate/RightPartyConnectRate;
		}

		public virtual TimeSpan AverageTaskHandlingTime()
		{
			var target = CallListLen*TargetRate/100.0;
			var rightPartyTotalHandlingTime = target*(RightPartyAverageHandlingTime + UnproductiveTime);
			var wrongPartyTotalHandlingTime = (target/RightPartyConnectRate*100 - target)*
														 (ConnectAverageHandlingTime + UnproductiveTime);
			var manualConnectingTime = (target*100*100/(ConnectRate*RightPartyConnectRate) - target/RightPartyConnectRate*100)*
												UnproductiveTime;
			var personHours = (rightPartyTotalHandlingTime + wrongPartyTotalHandlingTime + manualConnectingTime)/60/60;

			return TimeSpan.FromHours(personHours/CampaignTasks());
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual TimeSpan? GetManualProductionPlan(DateOnly date)
		{
			TimeSpan? manualTime = null;
			TimeSpan value;
			if (_manualProductionPlanDays.TryGetValue(date, out value))
				manualTime = value;

			return manualTime;
		}

		public virtual void SetManualProductionPlan(DateOnly date, TimeSpan time)
		{
			if (_manualProductionPlanDays.ContainsKey(date))
			{
				_manualProductionPlanDays[date] = time;
			}
			else
			{
				_manualProductionPlanDays.Add(date, time);
			}
		}

		public virtual void ClearProductionPlan(DateOnly date)
		{
			_manualProductionPlanDays.Remove(date);
		}

		public virtual void SetActualBacklog(DateOnly date, TimeSpan time)
		{
			if (_actualBacklogDays.ContainsKey(date))
			{
				_actualBacklogDays[date] = time;
			}
			else
			{
				_actualBacklogDays.Add(date, time);
			}
		}

		public virtual void ClearActualBacklog(DateOnly date)
		{
			_actualBacklogDays.Remove(date);
		}

		public virtual TimeSpan? GetActualBacklog(DateOnly date)
		{
			TimeSpan? time = null;
			TimeSpan value;
			if (_actualBacklogDays.TryGetValue(date, out value))
				time = value;

			return time;
		}

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IOutboundCampaign NoneEntityClone()
		{
			Campaign retobj = (Campaign) MemberwiseClone();
			retobj.SetId(null);
			CloneEvents(retobj);
			retobj._workingHours = new Dictionary<DayOfWeek, TimePeriod>();
			foreach (KeyValuePair<DayOfWeek, TimePeriod> keyValuePair in _workingHours)
			{
				TimePeriod template = keyValuePair.Value;
				retobj._workingHours.Add(keyValuePair.Key, template);
			}
			retobj._skill = Skill.NoneEntityClone();
			var workloads = Skill.WorkloadCollection;
			foreach (var workload in workloads)
			{
				retobj._skill.AddWorkload(workload.NoneEntityClone());
			}

			return retobj;
		}

		public virtual IOutboundCampaign EntityClone()
		{
			Campaign retobj = (Campaign) MemberwiseClone();
			CloneEvents(retobj);
			retobj._workingHours = new Dictionary<DayOfWeek, TimePeriod>();
			foreach (KeyValuePair<DayOfWeek, TimePeriod> keyValuePair in _workingHours)
			{
				TimePeriod template = keyValuePair.Value;
				retobj._workingHours.Add(keyValuePair.Key, template);
			}

			retobj._skill = Skill.EntityClone();
			var workloads = Skill.WorkloadCollection;
			foreach (var workload in workloads)
			{
				retobj._skill.AddWorkload(workload.EntityClone());
			}

			return retobj;
		}
	}
}
