using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Gamification
{
	public class RecalculateBadgeJobResultDetail
	{
		public Guid Id { get; set; }

		public string Owner { get; set; }

		public DateOnlyPeriod Period { get; set; }
		public DateTime CreateDateTime { get; set; }

		public string Status { get; set; }

		public bool HasError { get; set; }
		public string ErrorMessage { get; set; }
	}
}
