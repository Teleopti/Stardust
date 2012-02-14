using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class BusinessUnit : AggregateRoot, IBusinessUnit, IDeleteTag
    {
        #region Fields

        private Description _description;
        private readonly IList<ISite> _siteCollection;
        private bool _isDeleted;

        #endregion

        #region Constructor

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

        #endregion

        #region Interface

        #region IBusinessUnitHierarchyEntity Members

        /// <summary>
        /// Collects all the persons in the candidates that are in the Business Unit.
        /// </summary>
        /// <value></value>
        /// <returns>All persons in the business unit.</returns>
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateTimePeriod period)
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

        #endregion

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

		public virtual ReadOnlyCollection<ISite> SortedSiteCollection
		{
			get
			{
				var sortedList = (from site in _siteCollection
								  orderby site.Description.Name ascending
								  select site).ToList();

				return new ReadOnlyCollection<ISite>(sortedList);
			}
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
            return new ReadOnlyCollection<ITeam>((from
                                                    s in
                                                    SiteCollection
                                                        from
                                                    t in
                                                    s.TeamCollection
                                                        select
                                                    t).
                                                    ToList());
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
            //borde inte behövas då BU sätts till inloggad
            //site.Parent = this;
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

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
