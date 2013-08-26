﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class TeamOrSiteChangedMessageSenderTest
	{
		private IMessageSender _target;
		private MockRepository _mocks;
		private IServiceBusSender _serviceBusSender;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IServiceBusSender>();
			_target = new TeamOrSiteChangedMessageSender(_serviceBusSender);
		}

		[Test]
		public void ShouldSaveRebuildReadModelForTeamToQueue()
		{
			var team = new Team();
			var message = new PersonChangedMessage();
			
			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(team, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Send(message))
				      .Constraints(new Rhino.Mocks.Constraints.PredicateConstraint<PersonChangedMessage>(m => m.SerializedPeople == Guid.Empty.ToString()));
			}
			using (_mocks.Playback())
			{
				_target.Execute(roots);
			}
		}

		[Test]
		public void ShouldSaveRebuildReadModelForSiteToQueue()
		{
			var site = new Site("My Site");
			var message = new PersonChangedMessage();
			
			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(site, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Send(message))
				      .Constraints(new Rhino.Mocks.Constraints.PredicateConstraint<PersonChangedMessage>(m => m.SerializedPeople == Guid.Empty.ToString()));
			}
			using (_mocks.Playback())
			{
				_target.Execute(roots);
			}
		}
	}
}