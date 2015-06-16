using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAgentPortalSettingsQueryHandlerTest
	{
		private MockRepository mocks;
		private IPersonalSettingDataRepository personalSettingDataRepository;
        private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private GetAgentPortalSettingsQueryHandler target;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			personalSettingDataRepository = mocks.DynamicMock<IPersonalSettingDataRepository>();
            unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new GetAgentPortalSettingsQueryHandler(personalSettingDataRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetTheAgentPortalSettings()
		{
			var settings = new AgentPortalSettings {Resolution = 15};
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(personalSettingDataRepository.FindValueByKey("AgentPortalSettings", new AgentPortalSettings())).
					IgnoreArguments().Return(settings);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetAgentPortalSettingsQueryDto());
				result.First().Resolution.Should().Be.EqualTo(15);
			}
		}
	}
}
