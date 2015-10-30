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

    	public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
