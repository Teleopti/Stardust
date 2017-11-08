using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
			get => _person;
			set => _person = value;
		}
		
		public virtual BadgeType BadgeType
		{
			get => _badgeType;
			set => _badgeType = value;
		}

		public virtual int Amount
		{
			get => _amount;
			set => _amount = value;
		}

		public virtual DateOnly CalculatedDate
		{
			get => _calculatedDate;
			set => _calculatedDate = value;
		}

		public virtual string Description
		{
			get => _description;
			set => _description = value;
		}

		public virtual DateTime InsertedOn
		{
			get => _insertedOn;
			set => _insertedOn = value;
		}
	}
}