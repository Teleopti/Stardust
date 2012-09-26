using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetMultiplicatorShiftAllowanceQueryHandlerTest
	{

		private MockRepository mocks;
		private IMultiplicatorRepository multiplicatorRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetMultiplicatorShiftAllowanceQueryHandler target;
		private IMultiplicator multiplicator;
		private IList<IMultiplicator> multiplicatorList;
			
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multiplicatorRepository = mocks.DynamicMock<IMultiplicatorRepository>();
			multiplicator = new Multiplicator(MultiplicatorType.OBTime);
			multiplicatorList = new List<IMultiplicator>();
			multiplicatorList.Add(multiplicator);
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			target = new GetMultiplicatorShiftAllowanceQueryHandler(multiplicatorRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldReturnMultiplicatorSettingsForShiftAllowance()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			using (mocks.Record())
			{
				Expect.Call(multiplicatorRepository.LoadAllByTypeAndSortByName(MultiplicatorType.OBTime)).Return(multiplicatorList);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetMultiplicatorShiftAllowanceQueryDto());
				Assert.IsTrue(result.Count>0);
			}
		}
	}
}
