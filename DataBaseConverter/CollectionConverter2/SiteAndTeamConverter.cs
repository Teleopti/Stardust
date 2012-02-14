using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter2
{
    /// <summary>
    /// Class for creating oganisation conversion
    /// </summary>
    public class SiteAndTeamConverter
    {
        private readonly BusinessUnit _businessUnit;
        private readonly ICollection<Unit> _unitList;
        private readonly ObjectPairCollection<Unit, Site> _pairListUnitSite;
        private readonly ObjectPairCollection<UnitSub, Team> _pairListUnitSubTeam;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteAndTeamConverter"/> class.
        /// </summary>
        private SiteAndTeamConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteAndTeamConverter"/> class.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <param name="unitList">The unit reader.</param>
        public SiteAndTeamConverter(BusinessUnit businessUnit, ICollection<Unit> unitList) : this()
        {
            _businessUnit = businessUnit;
            _unitList = unitList;
            _pairListUnitSite = new ObjectPairCollection<Unit, Site>();
            _pairListUnitSubTeam = new ObjectPairCollection<UnitSub, Team>();
        }


        /// <summary>
        /// Gets the pair list of unitsub and team.
        /// </summary>
        /// <value>The pair list unit sub team.</value>
        public ObjectPairCollection<UnitSub, Team> PairListUnitSubTeam
        {
            get { return _pairListUnitSubTeam; }
        }

        /// <summary>
        /// Gets the pair list unit and site.
        /// </summary>
        /// <value>The pair list unit site.</value>
        public ObjectPairCollection<Unit, Site> PairListUnitSite
        {
            get { return _pairListUnitSite; }
        }

        /// <summary>
        /// Converts.
        /// </summary>
        public void Convert(Mapper<Site, Unit> siteMapper, 
                            Mapper<Team, UnitSub> teamMapper)
        {
            foreach (Unit unit in _unitList)
            {
                Site newSite = siteMapper.Map(unit);
                if (newSite != null)
                {
                    Unit oldUnit = unit;
                    PairListUnitSite.Add(oldUnit, newSite);
                    _businessUnit.AddSite(newSite);
                    foreach (UnitSub sub in unit.ChildUnits)
                    {
                        Team newTeam = teamMapper.Map(sub);
                        if (newTeam != null)
                        {
                            UnitSub oldTeam = sub;
                            PairListUnitSubTeam.Add(oldTeam, newTeam);
                            newSite.AddTeam(newTeam);
                        }
                    }
                }
            }
        }
    }
}