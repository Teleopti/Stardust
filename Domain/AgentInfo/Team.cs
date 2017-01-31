using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class Team : VersionedAggregateRoot, ITeam, IAggregateRootWithEvents, IDeleteTag
    {
        private Description _description;
        private ISite _site;
        private bool _isDeleted;
        private IScorecard _scorecard;

        public virtual bool IsChoosable => !IsDeleted;

	    public virtual Description Description => _description;

	    public virtual void SetDescription(Description value)
	    {
		    if (_description != value)
			    AddEvent(() => new TeamNameChangedEvent
			    {
				    TeamId = Id.GetValueOrDefault(),
				    Name = value.Name
			    });
		    _description = value;
	    }

	    public virtual ISite Site
        {
            get { return _site; }
            set
            {
				value.AddTeam(this);
	            _site = value;
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
                    return ServiceLocatorForEntity.CurrentBusinessUnit.Current();
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