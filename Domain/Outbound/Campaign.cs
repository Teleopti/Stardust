using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Outbound
{
	public enum CampaignStatus
	{
		Draft,
		Ongoing
	};

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
		private DateOnly _startDate;
		private DateOnly _endDate;
		private CampaignStatus _campaignStatus;
		private IEnumerable<CampaignWorkingPeriod> _campaignWorkingPeriods;
		private bool _isDeleted;

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

		public virtual DateOnly StartDate
		{
			get { return _startDate; }
			set { _startDate= value; }
		}

		public virtual DateOnly EndDate
		{
			get { return _endDate; }
			set { _endDate= value; }
		}

		public virtual CampaignStatus CampaignStatus
		{
			get { return _campaignStatus; }
			set { _campaignStatus = value; }
		}

		public virtual IEnumerable<CampaignWorkingPeriod> CampaignWorkingPeriods
		{
			get { return _campaignWorkingPeriods; }
			set { _campaignWorkingPeriods = value; }
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
