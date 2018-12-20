using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[Category("BucketB")]
	[DatabaseTest]
	public class Bug22329
	{
		public IPersonRequestRepository personRequestRepository;
		public IPersonRepository PersonRepository;
		
		[Test]
		public void ShouldNotGenerateUpdateWhenShiftTradeRequestIsRead()
		{
			Guid id;
			IPerson tradeWithPerson = SetupFixtureForAssembly.loggedOnPerson;
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPersonRequest request = new PersonRequest(SetupFixtureForAssembly.loggedOnPerson);
				IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
						new List<IShiftTradeSwapDetail>
											{
												new ShiftTradeSwapDetail(SetupFixtureForAssembly.loggedOnPerson, tradeWithPerson, new DateOnly(2008, 7, 16),
																								 new DateOnly(2008, 7, 16)),
											});
				request.Request = shiftTradeRequest;
				var pr = request;

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
		}
	}
}