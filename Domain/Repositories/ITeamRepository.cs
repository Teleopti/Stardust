using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{

    /// <summary>
    /// Defines the functionality of a .
    /// </summary>
    public interface ITeamRepository : IRepository<ITeam>
    {

        #region Properties - Instance Member

        #endregion

        #region Methods - Instance Member

        ICollection<ITeam> FindAllTeamByDescription();

        ICollection<ITeam> FindTeamByDescriptionName(string name);

        #endregion

    }

}
