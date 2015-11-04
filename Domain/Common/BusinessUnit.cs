using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class BusinessUnit : VersionedAggregateRoot, IBusinessUnit, IDeleteTag
    {
    	private Description _description;
        private readonly IList<ISite> _siteCollection;
        private bool _isDeleted;

    	/// <summary>
        /// CreateProjection new BusinessUnit
        /// </summary>
        public BusinessUnit(string nameToSet)
            : this()
        {
            _description = new Description(nameToSet);
        }

        /// <summary>
        /// Empty contructor for NHibernate
        /// </summary>
        protected BusinessUnit()
        {
            _siteCollection = new List<ISite>();
        }

    	/// <summary>
        /// Collects all the persons in the candidates that are in the Business Unit.
        /// </summary>
        /// <value></value>
        /// <returns>All persons in the business unit.</returns>
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            IList<IPerson> personsOnSite = new List<IPerson>();
            foreach (ISite site in _siteCollection)
            {
                ReadOnlyCollection<IPerson> personOnSite = site.PersonsInHierarchy(candidates, period);
                foreach (IPerson person in personOnSite)
                {
                    if (!personsOnSite.Contains(person))
                        personsOnSite.Add(person);
                }
            }
            return new ReadOnlyCollection<IPerson>(personsOnSite);
        }

    	/// <summary>
        /// Set/Get for description
        /// </summary>     
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Name Property
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _description.Name;
            }
            set
            {
                _description = new Description(value, _description.ShortName);
            }
        }

        /// <summary>
        /// ShortName Property
        /// </summary>
        public virtual string ShortName
        {
            get
            {
                return _description.ShortName;
            }
            set
            {
                _description = new Description(_description.Name, value);
            }
        }


        /// <summary>
        /// Gets the Sites.
        /// Read only wrapper around the actual site list.
        /// </summary>
        /// <value>The agents list.</value>
        public virtual ReadOnlyCollection<ISite> SiteCollection
        {
            get { return new ReadOnlyCollection<ISite>(_siteCollection); }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        /// <summary>
        /// Get the Teams that are is the BusinessUnit.
        /// </summary>
        /// <value>The team collection.</value>
        public virtual ReadOnlyCollection<ITeam> TeamCollection()
        {
        	return new ReadOnlyCollection<ITeam>(SiteCollection.SelectMany(s => s.TeamCollection).ToList());
        }

        /// <summary>
        /// Adds a Site.
        /// </summary>
        /// <param name="site">The site.</param>
        public virtual void AddSite(ISite site)
        {
            InParameter.NotNull("site", site);
            if (_siteCollection.Contains(site))
            {
                _siteCollection.Remove(site);
            }
            _siteCollection.Add(site);
        }

        /// <summary>
        /// Removeds the site.
        /// </summary>
        /// <param name="site">The site.</param>
        public virtual void RemoveSite(ISite site)
        {
            InParameter.NotNull("site", site);
            _siteCollection.Remove(site);
        }

        /// <summary>
        /// Finds the Site that param Team belongs to.
        /// </summary>
        /// <param name="searchedTeam">The team.</param>
        /// <returns></returns>
        public virtual ISite FindTeamSite(IEntity searchedTeam)
        {
            foreach (ISite site in SiteCollection)
            {
                foreach (ITeam team in site.TeamCollection)
                {
                    if (team.Equals(searchedTeam))
                        return site;
                }
            }
            return null;
        }

    	public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
