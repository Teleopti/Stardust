using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	public class GetScheduleChangesSubscriptionSettingsQueryHandlerTest
	{
		[Test]
		public void ShouldGetScheduleChangesSubscriptionSettings()
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			var target = new GetScheduleChangesSubscriptionSettingsQueryHandler(globalSettingDataRepository, unitOfWorkFactory);

			var settings = new ScheduleChangeSubscriptions();
			settings.Add(new ScheduleChangeListener {Name = "Facebook",RelativeDateRange = new MinMax<int>(-2,2),Uri = new Uri("http://myserver.com/schedule")});

			globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, settings);

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = target.Handle(new GetScheduleChangesSubscriptionSettingsQueryDto());
				result.Count.Should().Be.EqualTo(1);

				var firstListener = result.First().Listeners.First();
				firstListener.Name.Should().Be.EqualTo("Facebook");
				firstListener.DaysStartFromCurrentDate.Should().Be.EqualTo(-2);
				firstListener.DaysEndFromCurrentDate.Should().Be.EqualTo(2);
				firstListener.Url.Should().Be.EqualTo("http://myserver.com/schedule");
			}
		}

		[Test]
		public void ShouldIncludePublicKeyForSignatureValidation()
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			var target = new GetScheduleChangesSubscriptionSettingsQueryHandler(globalSettingDataRepository, unitOfWorkFactory);

			var settings = new ScheduleChangeSubscriptions();
			settings.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(-2, 2), Uri = new Uri("http://myserver.com/schedule") });

			globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, settings);

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = target.Handle(new GetScheduleChangesSubscriptionSettingsQueryDto());
				result.Count.Should().Be.EqualTo(1);

				var subscription = result.First();
				subscription.Modulus.Should().Be.EqualTo(
					"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w==");
				subscription.Exponent.Should().Be.EqualTo("AQAB");
			}
		}

		[Test]
		public void ShouldRequirePermissionsToGetTheSubscriptions()
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			var target = new GetScheduleChangesSubscriptionSettingsQueryHandler(globalSettingDataRepository, unitOfWorkFactory);

			var settings = new ScheduleChangeSubscriptions();
			settings.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(-2, 2), Uri = new Uri("http://myserver.com/schedule") });

			globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, settings);

			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				Assert.Throws<FaultException>(() => target.Handle(new GetScheduleChangesSubscriptionSettingsQueryDto()));
			}
		}
	}
}