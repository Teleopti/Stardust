using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetMultiplicatorOvertimeQueryHandlerTest
	{
		private MockRepository mocks;
		private IMultiplicatorRepository multiplicatorRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetMultiplicatorOvertimeQueryHandler target;
		private IMultiplicator multiplicator;
		private IList<IMultiplicator> multiplicatorList;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multiplicatorRepository = mocks.DynamicMock<IMultiplicatorRepository>();
			multiplicator = new Multiplicator(MultiplicatorType.Overtime);
			multiplicator.Description = new Description("Overtime", "OV");
			multiplicatorList = new List<IMultiplicator>();
			multiplicatorList.Add(multiplicator);
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new GetMultiplicatorOvertimeQueryHandler(multiplicatorRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldReturnMultiplicatorSettingsForOvertime()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			using (mocks.Record())
			{
				Expect.Call(multiplicatorRepository.LoadAllByTypeAndSortByName(MultiplicatorType.Overtime)).Return(multiplicatorList);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetMultiplicatorOvertimeQueryDto());
				var first = result.ToList().ElementAt(0);
				Assert.IsTrue(result.Count > 0);
				Assert.AreEqual(first.ShortName, "OV");
				Assert.AreEqual(first.Name, "Overtime");
				Assert.AreEqual(first.MultiplicatorType, (MultiplicatorTypeDto)MultiplicatorType.Overtime);
			}
		} 
	}
}
