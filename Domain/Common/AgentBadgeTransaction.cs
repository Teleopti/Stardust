using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeTransaction : SimpleAggregateRoot, IAgentBadgeTransaction
	{
		private IPerson _person;
		private BadgeType _badgeType;
		private int _amount;
		private DateOnly _calculatedDate;
		private string _description;
		private DateTime _insertedOn;

		public virtual IPerson Person
		{
			get { return _person; }
			set { _person = value; }
		}
		
		public virtual BadgeType BadgeType
		{
			get { return _badgeType; }
			set { _badgeType = value; }
		}

		public virtual int Amount
		{
			get { return _amount; }
			set { _amount = value; }
		}

		public virtual DateOnly CalculatedDate
		{
			get { return _calculatedDate; }
			set { _calculatedDate = value; }
		}

		public virtual string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public virtual DateTime InsertedOn
		{
			get { return _insertedOn; }
			set { _insertedOn = value; }
		}
	}
}