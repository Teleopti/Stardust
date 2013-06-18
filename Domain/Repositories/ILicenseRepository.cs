using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Used for persisting and retrieving XML based Licenses to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public interface ILicenseRepository : IRepository<ILicense>
    {
	    IList<ActiveAgent> GetActiveAgents();
    }

	public class ActiveAgent
	{
		public string BusinessUnit { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string EmploymentNumber { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? LeavingDate { get; set; }
	}
}
