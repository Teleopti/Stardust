using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.DayOff;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.DayOff
{
	[TestFixture]
	public class DayOffTemplateChangedHandlerTests
	{
		private AnalyticsDayOffUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private IDayOffTemplateRepository _dayOffTemplateRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsDayOffRepository = new FakeAnalyticsDayOffRepository();
			_dayOffTemplateRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();

			_target = new AnalyticsDayOffUpdater(_analyticsBusinessUnitRepository, _analyticsDayOffRepository, _dayOffTemplateRepository);
		}

		[Test]
		public void ShouldHandleEventAndAdd()
		{
			var id = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffTemplateId = id,
				LogOnBusinessUnitId = businessUnitCode
			};

			var dayOffTemplate = new DayOffTemplate { UpdatedOn = DateTime.Today };
			dayOffTemplate.SetId(id);
			dayOffTemplate.ChangeDescription("DayOffName", "DD");

			_dayOffTemplateRepository.Stub(x => x.Get(id)).Return(dayOffTemplate);

			_target.Handle(@event);

			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			var analyticsDayOff = _analyticsDayOffRepository.DayOffs().First();
			analyticsDayOff.DayOffCode.Should().Be.EqualTo(dayOffTemplate.Id.GetValueOrDefault());
			analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
			analyticsDayOff.DatasourceUpdateDate.Should().Be.EqualTo(dayOffTemplate.UpdatedOn);
			analyticsDayOff.DisplayColor.Should().Be.EqualTo(dayOffTemplate.DisplayColor.ToArgb());
			analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo(ColorTranslator.ToHtml(dayOffTemplate.DisplayColor));
			analyticsDayOff.DatasourceId.Should().Be.EqualTo(1);
			analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(_analyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}

		[Test]
		public void ShouldHandleUpdateEvent()
		{
			var id = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffTemplateId = id,
				LogOnBusinessUnitId = businessUnitCode
			};

			var dayOffTemplate = new DayOffTemplate { UpdatedOn = DateTime.Today };
			dayOffTemplate.SetId(id);
			dayOffTemplate.ChangeDescription("DayOffName", "DD");

			_dayOffTemplateRepository.Stub(x => x.Get(id)).Return(dayOffTemplate);

			_target.Handle(@event);
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);

			dayOffTemplate.ChangeDescription("DayOffName update", "DD");
			_dayOffTemplateRepository.Stub(x => x.Get(id)).Return(dayOffTemplate);

			_target.Handle(@event);

			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			var analyticsDayOff = _analyticsDayOffRepository.DayOffs().First();
			analyticsDayOff.DayOffCode.Should().Be.EqualTo(dayOffTemplate.Id.GetValueOrDefault());
			analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
			analyticsDayOff.DatasourceUpdateDate.Should().Be.EqualTo(dayOffTemplate.UpdatedOn);
			analyticsDayOff.DisplayColor.Should().Be.EqualTo(dayOffTemplate.DisplayColor.ToArgb());
			analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo(ColorTranslator.ToHtml(dayOffTemplate.DisplayColor));
			analyticsDayOff.DatasourceId.Should().Be.EqualTo(1);
			analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(_analyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}
	}
}
