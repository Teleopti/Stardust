using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.DayOff;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.DayOff
{
	[TestFixture]
	public class DayOffTemplateChangedHandlerTests
	{
		private AnalyticsDayOffUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsDayOffRepository _analyticsDayOffRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsDayOffRepository = new FakeAnalyticsDayOffRepository();

			_target = new AnalyticsDayOffUpdater(_analyticsBusinessUnitRepository, _analyticsDayOffRepository);
		}

		[Test]
		public void ShouldHandleEventAndAdd()
		{
			var id = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffName = "DayOffName",
				DayOffShortName = "DD",
				DayOffTemplateId = id,
				DatasourceUpdateDate = DateTime.Today,
				LogOnBusinessUnitId = businessUnitCode
			};
			_target.Handle(@event);
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().DayOffCode.Should().Be.EqualTo(@event.DayOffTemplateId);
			_analyticsDayOffRepository.DayOffs().First().DayOffName.Should().Be.EqualTo(@event.DayOffName);
			_analyticsDayOffRepository.DayOffs().First().DayOffShortname.Should().Be.EqualTo(@event.DayOffShortName);
			_analyticsDayOffRepository.DayOffs().First().DatasourceUpdateDate.Should().Be.EqualTo(DateTime.Today);
			_analyticsDayOffRepository.DayOffs().First().DisplayColor.Should().Not.Be.EqualTo(null);
			_analyticsDayOffRepository.DayOffs().First().DisplayColorHtml.Should().Not.Be.EqualTo(null);
			_analyticsDayOffRepository.DayOffs().First().DatasourceId.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().BusinessUnitId.Should().Be.EqualTo(_analyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}

		[Test]
		public void ShouldHandleUpdateEvent()
		{
			var id = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(0);
			var @event = new DayOffTemplateChangedEvent
			{
				DayOffName = "DayOffName",
				DayOffShortName = "DD",
				DayOffTemplateId = id,
				DatasourceUpdateDate = DateTime.Today,
				LogOnBusinessUnitId = businessUnitCode
			};
			_target.Handle(@event);
			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().DayOffName.Should().Be.EqualTo(@event.DayOffName);

			@event.DayOffName = "DayOffName update";
			_target.Handle(@event);

			_analyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().DayOffCode.Should().Be.EqualTo(@event.DayOffTemplateId);
			_analyticsDayOffRepository.DayOffs().First().DayOffName.Should().Be.EqualTo(@event.DayOffName);
			_analyticsDayOffRepository.DayOffs().First().DayOffShortname.Should().Be.EqualTo(@event.DayOffShortName);
			_analyticsDayOffRepository.DayOffs().First().DatasourceUpdateDate.Should().Be.EqualTo(DateTime.Today);
			_analyticsDayOffRepository.DayOffs().First().DisplayColor.Should().Not.Be.EqualTo(null);
			_analyticsDayOffRepository.DayOffs().First().DisplayColorHtml.Should().Not.Be.EqualTo(null);
			_analyticsDayOffRepository.DayOffs().First().DatasourceId.Should().Be.EqualTo(1);
			_analyticsDayOffRepository.DayOffs().First().BusinessUnitId.Should().Be.EqualTo(_analyticsBusinessUnitRepository.Get(businessUnitCode).BusinessUnitId);
		}
	}
}
