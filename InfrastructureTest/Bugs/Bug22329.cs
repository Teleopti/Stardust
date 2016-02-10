using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug22329 : DatabaseTest
	{
		private IPersonRequestRepository personRequestRepository;

		protected override void SetupForRepositoryTest()
		{
			personRequestRepository = new PersonRequestRepository(new FromFactory(() => SetupFixtureForAssembly.DataSource.Application));
			
			//make sure setup data is persisted
			CleanUpAfterTest();
			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldNotGenerateUpdateWhenShiftTradeRequestIsRead()
		{
			Guid id;
			//save
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var pr = createShiftTradeRequest();
				personRequestRepository.Add(pr);
				id = pr.Id.Value;
				uow.PersistAll();
			}
			//load, change nothing and commit. 
			int version;
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var pr = personRequestRepository.Get(id);
				version = ((IVersioned)pr).Version.Value;
				pr.Request.Root(); //make sure shift trade is loaded by calling whatever on the IRequest
				uow.PersistAll();
			}
			//Version number should not be increased
			using (SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				((IVersioned) personRequestRepository.Get(id)).Version.Value
					.Should().Be.EqualTo(version);
			}

			cleanup(id);
		}

		private void cleanup(Guid id)
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				personRequestRepository.Remove(personRequestRepository.Find(id));
				uow.PersistAll();
			}
		}


		private IPersonRequest createShiftTradeRequest()
		{
			IPersonRequest request = new PersonRequest(SetupFixtureForAssembly.loggedOnPerson);
			IPerson tradeWithPerson = SetupFixtureForAssembly.loggedOnPerson;
			PersistAndRemoveFromUnitOfWork(tradeWithPerson);
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
					new List<IShiftTradeSwapDetail>
										{
												new ShiftTradeSwapDetail(SetupFixtureForAssembly.loggedOnPerson, tradeWithPerson, new DateOnly(2008, 7, 16),
																								 new DateOnly(2008, 7, 16)),
										});
			request.Request = shiftTradeRequest;
			return request;
		}
	}
}