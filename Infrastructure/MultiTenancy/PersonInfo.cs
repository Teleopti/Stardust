using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PersonInfo
	{
		private DateOnly? terminalDate;
		private bool isDeleted;
		private string applicationLogonName;
		private string identity;

		public virtual Guid Id { get; set; }
		public virtual string Password { get; set; }

		public virtual string Tennant
		{
			get { return "Teleopti WFM"; }
		}
	}
}