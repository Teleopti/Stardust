using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	/// <summary>
	/// Loads and sets up role for a user.  
	/// </summary>
	public class RoleForUser : IUserSetup, IUserRoleSetup
	{
		public string Name { get; set; }
		public string Team { get; set; }

		/// <summary>
		/// Loads the role by the name given in the Name property and adds the role to the given user.
		/// </summary>
		/// <param name="uow">The uow.</param>
		/// <param name="user">The user.</param>
		/// <param name="cultureInfo">The culture info.</param>
		/// <remarks>This method does not do persist. There is NO error handling for ungiven or inproper role name.</remarks>
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var roleRepository = new ApplicationRoleRepository(uow);
			var role = roleRepository.LoadAll().Single(b => b.Name == Name);

			if (!string.IsNullOrEmpty(Team))
			{
				var teamRepository = new TeamRepository(uow);
				var teams = teamRepository.FindTeamByDescriptionName(Team);
				foreach (var team in teams)
				{
					role.AvailableData.AddAvailableTeam(team);
				}
			}

			user.PermissionInformation.AddApplicationRole(role);
		}
	}

}