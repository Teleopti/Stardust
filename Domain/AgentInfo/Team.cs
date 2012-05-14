using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Class for team
    /// </summary>
    public class Team : AggregateRoot, ITeam, IDeleteTag
    {
        private Description _description;
        private ISite _site;
        private bool _isDeleted;
        private IScorecard _scorecard;

        /// <summary>
        /// Collects all the persons in the candidates that are in the Team.
        /// </summary>
        /// <value></value>
        /// <returns>All persons in the team.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.CompareTo(System.String)")]
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            List<IPerson> personInTeamCollection = new List<IPerson>(candidates)
                .FindAll(new PersonBelongsToTeamSpecification(period, this).IsSatisfiedBy);
            personInTeamCollection.Sort(delegate(IPerson p1, IPerson p2) { return p1.Name.ToString(NameOrderOption.LastNameFirstName).CompareTo(p2.Name.ToString(NameOrderOption.LastNameFirstName)); });
            return new ReadOnlyCollection<IPerson>(personInTeamCollection);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        public virtual bool IsChoosable
        {
            get { return !IsDeleted; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>The site.</value>
        public virtual ISite Site
        {
            get { return _site; }
            set { _site = value; }
        }

        public virtual string SiteAndTeam
        {
            get{ return string.Concat(_site.Description.Name, "/", _description.Name);}
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual IBusinessUnit BusinessUnitExplicit
        {
            get
            {
                if (_site == null)
                    return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
                return _site.BusinessUnit;
            }
        }

        public virtual IScorecard Scorecard
        {
            get { return _scorecard; }
            set { _scorecard = value; }
        }
    }
}
