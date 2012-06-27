﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class GroupPageChangedMessageSenderTest
	{
		private IMessageSender _target;
		private MockRepository _mocks;
        private ISaveToDenormalizationQueue _saveToDenormalizationQueue;
		private ISendDenormalizeNotification _sendDenormalizeNotification;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _saveToDenormalizationQueue = _mocks.DynamicMock<ISaveToDenormalizationQueue>();

			_sendDenormalizeNotification = _mocks.DynamicMock<ISendDenormalizeNotification>();
			_target = new GroupPageChangedMessageSender(_sendDenormalizeNotification, _saveToDenormalizationQueue);
		}

        [Test]
        public void ShouldSaveRebuildReadModelForGroupPageToQueue()
        {
            var session = _mocks.DynamicMock<IRunSql>();
            var page = new GroupPage("Page");
            Guid[] ids = new Guid[] { };
            var message = new GroupPageChangedMessage();
            message.SetGroupPageIdCollection(ids);

            var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(page, DomainUpdateType.Update);

            using (_mocks.Record())
            {
                //Expect.Call(() => _saveToDenormalizationQueue.Execute(message, session)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Execute(session, roots);
            }
        }


		[Test]
		public void ShouldNotRebuildReadModelForScenario()
		{
			var session = _mocks.DynamicMock<IRunSql>();
			var scenario = _mocks.DynamicMock<IScenario>();

			using (_mocks.Record())
			{
			}
			using (_mocks.Playback())
			{
				_target.Execute(session, new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
			}
		}
	}
}