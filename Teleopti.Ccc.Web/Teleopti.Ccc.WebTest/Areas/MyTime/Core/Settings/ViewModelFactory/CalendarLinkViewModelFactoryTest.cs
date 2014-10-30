using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.ViewModelFactory
{
	[TestFixture]
	public class CalendarLinkViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateViewModel()
		{
			var context = new FakeHttpContext("/");
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			const string actionName = "actionName";
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/" + actionName));
			context.SetRequest(request);
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(context);
			var calendarLinkIdGenerator = MockRepository.GenerateMock<ICalendarLinkIdGenerator>();
			const string calendarLinkId = "calendarLinkId";
			calendarLinkIdGenerator.Stub(x => x.Generate()).Return(calendarLinkId);
			var target = new CalendarLinkViewModelFactory(currentHttpContext, calendarLinkIdGenerator);

			var result = target.CreateViewModel(new CalendarLinkSettings
				{
					IsActive = true
				}, actionName);
			result.IsActive.Should().Be.True();
			result.Url.Should()
			      .Be.EqualTo("http://xxx.xxx.xxx.xxx/Mytime/Share?id=" + calendarLinkId);
		}
	}
}