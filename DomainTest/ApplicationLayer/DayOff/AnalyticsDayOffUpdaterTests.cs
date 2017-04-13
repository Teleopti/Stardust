using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.DayOff;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.DayOff
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsDayOffUpdaterTests : ISetup
	{
		public AnalyticsDayOffUpdater Target;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public IAnalyticsDayOffRepository AnalyticsDayOffRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsDayOffUpdater>();
		}

		[Test]
		public void ShouldHandleEventAndAdd()
		{
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var id = Guid.NewGuid();
			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffTemplateId = id,
				LogOnBusinessUnitId = businessUnitCode
			};

			var dayOffTemplate = new DayOffTemplate { UpdatedOn = DateTime.Today }.WithId(id);
			dayOffTemplate.ChangeDescription("DayOffName", "DD");
			DayOffTemplateRepository.Add(dayOffTemplate);

			Target.Handle(@event);

			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			var analyticsDayOff = AnalyticsDayOffRepository.DayOffs().First();
			analyticsDayOff.DayOffCode.Should().Be.EqualTo(dayOffTemplate.Id.GetValueOrDefault());
			analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
			analyticsDayOff.DatasourceUpdateDate.Should().Be.EqualTo(dayOffTemplate.UpdatedOn);
			analyticsDayOff.DisplayColor.Should().Be.EqualTo(dayOffTemplate.DisplayColor.ToArgb());
			analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo(ColorTranslator.ToHtml(dayOffTemplate.DisplayColor));
			analyticsDayOff.DatasourceId.Should().Be.EqualTo(1);
			analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(AnalyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}

		[Test]
		public void ShouldHandleUpdateEvent()
		{
			var id = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));
			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffTemplateId = id,
				LogOnBusinessUnitId = businessUnitCode
			};

			var dayOffTemplate = new DayOffTemplate { UpdatedOn = DateTime.Today }.WithId(id);
			dayOffTemplate.ChangeDescription("DayOffName", "DD");
			DayOffTemplateRepository.Add(dayOffTemplate);

			Target.Handle(@event);
			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			AnalyticsDayOffRepository.DayOffs().First().DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);

			dayOffTemplate.ChangeDescription("DayOffName update", "DD");

			Target.Handle(@event);

			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			var analyticsDayOff = AnalyticsDayOffRepository.DayOffs().First();
			analyticsDayOff.DayOffCode.Should().Be.EqualTo(dayOffTemplate.Id.GetValueOrDefault());
			analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
			analyticsDayOff.DatasourceUpdateDate.Should().Be.EqualTo(dayOffTemplate.UpdatedOn);
			analyticsDayOff.DisplayColor.Should().Be.EqualTo(dayOffTemplate.DisplayColor.ToArgb());
			analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo(ColorTranslator.ToHtml(dayOffTemplate.DisplayColor));
			analyticsDayOff.DatasourceId.Should().Be.EqualTo(1);
			analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(AnalyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}
	}
}
