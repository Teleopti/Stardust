using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Gamification
{
	public class RecalculateBadgeJobResultDetail
	{
		public Guid Id { get; set; }

		public string Owner { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }	

		public DateTime CreateDateTime { get; set; }

		public string Status { get; set; }

		public bool HasError { get; set; }
		public string ErrorMessage { get; set; }
	}
}
