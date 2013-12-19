using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug26224 : DatabaseTest
	{
		private IRtaStateGroupRepository stateGroupRepository;

		protected override void SetupForRepositoryTest()
		{
			stateGroupRepository = new RtaStateGroupRepository(SetupFixtureForAssembly.DataSource.Application);
			
			//make sure setup data is persisted
			SkipRollback();
			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldNotGenerateUpdateWhenRtaStateGroupIsRead()
		{
			Guid id;
			int version;
			//save
			var pr = createRtaStateGroupWithOneState();
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				stateGroupRepository.Add(pr);
				id = pr.Id.Value;
				uow.PersistAll();
				version = ((IVersioned)pr).Version.Value;
			}
			//load, clone and commit. 
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				//uow.Reassociate(pr);
				uow.Merge(pr.EntityClone());
				uow.PersistAll();
			}
			//Version number should not be increased
			using (SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				((IVersioned) stateGroupRepository.Get(id)).Version.Value
					.Should().Be.EqualTo(version);
			}

			cleanup(id);
		}

		private void cleanup(Guid id)
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				stateGroupRepository.Remove(stateGroupRepository.Get(id));
				uow.PersistAll();
			}
		}

		private IRtaStateGroup createRtaStateGroupWithOneState()
		{
			var group = new RtaStateGroup("Default", true, true);
			group.AddState("Ready", "Ready", Guid.Empty);
			return group;
		}
	}
}