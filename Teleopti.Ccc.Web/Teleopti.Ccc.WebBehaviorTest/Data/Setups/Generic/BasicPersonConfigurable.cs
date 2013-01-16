using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class BasicPersonConfigurable : IUserDataSetup
	{
		public DateTime? TerminalDate { get; set; }
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			if (!string.IsNullOrEmpty(Name))
				user.Name = extractName();

			user.TerminalDate = TerminalDate.HasValue ? new DateOnly(TerminalDate.Value) : null;
		}

		private Name extractName()
		{
			var parts = Name.Split(' ');
			string firstName;
			string lastName;
			if (parts.Length > 1)
			{
				firstName = parts[0];
				lastName = string.Join(" ", parts, 1, parts.Length - 1);
			}
			else
			{
				firstName = Name;
				lastName = Name;
			}

			return new Name(firstName,lastName);
		}
	}
}