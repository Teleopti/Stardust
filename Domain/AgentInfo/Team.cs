using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class Team : AggregateRoot_Events_ChangeInfo_Versioned, ITeam, IAggregateRoot_Events, IDeleteTag
	{
		private Description _description;
		private ISite _site;
		private bool _isDeleted;
		private IScorecard _scorecard;

		public override IEnumerable<IEvent> PopAllEvents(IPopEventsContext context) =>
			base.PopAllEvents(context)
				.KeepLastOfType<TeamNameChangedEvent>();

		public virtual bool IsChoosable => !IsDeleted;

		public virtual Description Description => _description;

		public virtual void SetDescription(Description value)
		{
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

		public virtual string SiteAndTeam => string.Concat(_site.Description.Name, "/", _description.Name);

		public virtual bool IsDeleted => _isDeleted;

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual IBusinessUnit BusinessUnitExplicit => _site?.BusinessUnit ?? ServiceLocator_DONTUSE.CurrentBusinessUnit.Current();

		public virtual IScorecard Scorecard
		{
			get { return _scorecard; }
			set { _scorecard = value; }
		}
	}
}