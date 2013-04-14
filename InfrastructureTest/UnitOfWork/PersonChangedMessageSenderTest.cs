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
	public class PersonChangedMessageSenderTest
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
			_target = new PersonChangedMessageSender(_sendDenormalizeNotification, _saveToDenormalizationQueue);
		}

        [Test]
        public void ShouldSaveRebuildReadModelForPersonToQueue()
        {
            var person = new Person();
            Guid[] ids = new Guid[] {};
            var message = new PersonChangedMessage();
            message.SetPersonIdCollection(ids);
            
            var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(person, DomainUpdateType.Update);

            using (_mocks.Record())
            {
                Expect.Call(() => _saveToDenormalizationQueue.Execute(message)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Execute(roots);
            }
        }

		[Test]
		public void ShouldSaveRebuildReadModelForPersonWriteProtectionToQueue()
		{
			var session = _mocks.DynamicMock<IRunSql>();
			var personWriteProtectionInfo = new PersonWriteProtectionInfo(new Person());
			Guid[] ids = new Guid[] { };
			var message = new PersonChangedMessage();
			message.SetPersonIdCollection(ids);

			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(personWriteProtectionInfo, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _saveToDenormalizationQueue.Execute(message, session)).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Execute(session, roots);
			}
		}

        [Test]
        public void ShouldNotRebuildReadModelForScenario()
        {
            var scenario = _mocks.DynamicMock<IScenario>();

            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                _target.Execute(new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
            }
        }
	}

	
}
