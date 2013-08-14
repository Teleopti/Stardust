using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class Team : AggregateRoot, ITeam, IAggregateRootWithEvents, IDeleteTag
    {
        private Description _description;
        private ISite _site;
        private bool _isDeleted;
        private IScorecard _scorecard;

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.CompareTo(System.String)")]
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            List<IPerson> personInTeamCollection = new List<IPerson>(candidates)
                .FindAll(new PersonBelongsToTeamSpecification(period, this).IsSatisfiedBy);
            personInTeamCollection.Sort(
	            (p1, p2) =>
	            p1.Name.ToString(NameOrderOption.LastNameFirstName)
	              .CompareTo(p2.Name.ToString(NameOrderOption.LastNameFirstName)));
            return new ReadOnlyCollection<IPerson>(personInTeamCollection);
        }

        public virtual bool IsChoosable
        {
            get { return !IsDeleted; }
        }

        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual ISite Site
        {
            get { return _site; }
            set
            {
	            if (_site == value) return;

	            string siteBefore = null;
	            if (_site != null)
	            {
		            siteBefore = XmlConvert.ToString(_site.Id.GetValueOrDefault());
	            }
	            _site = value;
	            var siteAfter = XmlConvert.ToString(_site.Id.GetValueOrDefault());
	            AddEvent(new TeamChangedEvent
		            {
			            TeamId = Id.GetValueOrDefault(),
			            Property = "Site",
			            OldValue = siteBefore,
			            NewValue = siteAfter
		            });
            }
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
