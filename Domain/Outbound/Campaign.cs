using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class Campaign : NonversionedAggregateRootWithBusinessUnit, IDeleteTag
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
		private DateOnlyPeriod _spanningPeriod;

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

		public virtual DateOnlyPeriod SpanningPeriod
		{
			get { return _spanningPeriod; }
			set { _spanningPeriod = value; }
		}

		public virtual int CampaignTasks()
		{
			if (RightPartyConnectRate == 0)
				return 0;

			return CallListLen*TargetRate/RightPartyConnectRate;
		}

		public virtual TimeSpan AverageTaskHandlingTime()
		{
			var target = CallListLen*TargetRate/100;
			var rightPartyTotalHandlingTime = target*(RightPartyAverageHandlingTime + UnproductiveTime);
			var wrongPartyTotalHandlingTime = (target/RightPartyConnectRate*100 - target)*(ConnectAverageHandlingTime+UnproductiveTime);
			var manualConnectingTime = (target*100*100/(ConnectRate*RightPartyConnectRate) - target/RightPartyConnectRate*100)*UnproductiveTime;
			var personHours = (double)(rightPartyTotalHandlingTime + wrongPartyTotalHandlingTime + manualConnectingTime)/60/60;

			return TimeSpan.FromHours(personHours / CampaignTasks());
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
			if(_manualProductionPlanDays.ContainsKey(date))
				manualTime = _manualProductionPlanDays[date];
				
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
			if (_actualBacklogDays.ContainsKey(date))
				time = _actualBacklogDays[date];

			return time;
		}
	}
}
