using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	public class GetScheduleChangesSubscriptionSettingsQueryHandlerTest
	{
		[Test]
		public void ShouldGetScheduleChangesSubscriptionSettings()
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new GetScheduleChangesSubscriptionSettingsQueryHandler(globalSettingDataRepository, unitOfWorkFactory);

			var settings = new ScheduleChangeSubscriptions();
			settings.Add(new ScheduleChangeListener {Name = "Facebook",RelativeDateRange = new MinMax<int>(-2,2),Uri = new Uri("http://myserver.com/schedule")});

			globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, settings);

			var result = target.Handle(new GetScheduleChangesSubscriptionSettingsQueryDto());
			result.Count.Should().Be.EqualTo(1);

			var firstListener = result.First().Listeners.First();
			firstListener.Name.Should().Be.EqualTo("Facebook");
			firstListener.DaysStartFromCurrentDate.Should().Be.EqualTo(-2);
			firstListener.DaysEndFromCurrentDate.Should().Be.EqualTo(2);
			firstListener.Url.Should().Be.EqualTo("http://myserver.com/schedule");
		}

		[Test, ExpectedException(typeof(FaultException))]
		public void ShouldRequirePermissionsToGetTheSubscriptions()
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new GetScheduleChangesSubscriptionSettingsQueryHandler(globalSettingDataRepository, unitOfWorkFactory);

			var settings = new ScheduleChangeSubscriptions();
			settings.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(-2, 2), Uri = new Uri("http://myserver.com/schedule") });

			globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, settings);

			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
			{
				target.Handle(new GetScheduleChangesSubscriptionSettingsQueryDto());
			}
		}
	}
}