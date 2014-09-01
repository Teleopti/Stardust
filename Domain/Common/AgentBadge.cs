using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge : SimpleAggregateRoot, IAgentBadge
	{
		private IPerson _person;
		private BadgeType _badgeType;

		private int _bronzeBadge;
		private int _silverBadge;
		private int _goldBadge;

		private DateOnly _lastCalculatedDate;

		public virtual IPerson Person
		{
			get { return _person; }
			set { _person = value; }
		}

		public virtual int BronzeBadge
		{
			get { return _bronzeBadge; }
			set { _bronzeBadge = value; }
		}

		public virtual int SilverBadge
		{
			get { return _silverBadge; }
			set { _silverBadge = value; }
		}

		public virtual int GoldBadge
		{
			get { return _goldBadge; }
			set { _goldBadge = value; }
		}

		public virtual BadgeType BadgeType
		{
			get { return _badgeType; }
			set { _badgeType = value; }
		}

		public virtual DateOnly LastCalculatedDate
		{
			get { return _lastCalculatedDate; }
			set { _lastCalculatedDate = value; }
		}

		public virtual bool BronzeBadgeAdded { get; set; }
		public virtual bool SilverBadgeAdded { get; set; }
		public virtual bool GoldBadgeAdded { get; set; }

		/// <summary>
		/// Add a new badge to current badge
		/// </summary>
		/// <param name="newBadge">New badge</param>
		/// <param name="silverToBronzeBadgeRate">The rate exchange bronze badge to silver badge.</param>
		/// <param name="goldToSilverBadgeRate">The rate exchange silver badge to gold badge.</param>
		public virtual void AddBadge(IAgentBadge newBadge, int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			InParameter.MustBeTrue("newBadge", newBadge.BadgeType == BadgeType);
			InParameter.ValueMustBeLargerThanZero("silverToBronzeBadgeRate", silverToBronzeBadgeRate);
			InParameter.ValueMustBeLargerThanZero("goldToSilverBadgeRate", goldToSilverBadgeRate);

			BronzeBadge += newBadge.BronzeBadge;
			SilverBadge += newBadge.SilverBadge;
			GoldBadge += newBadge.GoldBadge;

			BronzeBadgeAdded = newBadge.BronzeBadgeAdded;
			SilverBadgeAdded = newBadge.SilverBadgeAdded;
			GoldBadgeAdded = newBadge.GoldBadgeAdded;
			
			LastCalculatedDate = newBadge.LastCalculatedDate;

			//Update according to rate.
			if (BronzeBadge >= silverToBronzeBadgeRate)
			{
				SilverBadge = SilverBadge + BronzeBadge / silverToBronzeBadgeRate;
				SilverBadgeAdded = true;
				BronzeBadge = BronzeBadge % silverToBronzeBadgeRate;
				BronzeBadgeAdded = false;
			}

			if (SilverBadge >= goldToSilverBadgeRate)
			{
				GoldBadge = GoldBadge + SilverBadge / goldToSilverBadgeRate;
				GoldBadgeAdded = true;
				SilverBadge = SilverBadge % goldToSilverBadgeRate;
				SilverBadgeAdded = false;
			}
		}
	}
}