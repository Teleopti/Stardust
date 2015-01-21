using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PersonInfo
	{
#pragma warning disable 169
		private DateOnly? terminalDate;
		private bool isDeleted;
		private string applicationLogonName;
		private string identity;
#pragma warning restore 169

		public virtual Guid Id { get; set; }
		public virtual string Password { get; set; }

		public virtual string Tennant
		{
			get { return "Teleopti WFM"; }
		}
	}
}