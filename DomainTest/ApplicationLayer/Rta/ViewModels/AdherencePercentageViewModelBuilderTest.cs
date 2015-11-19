using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[IoCTest]
	[TestFixture]
	public class AdherencePercentageViewModelBuilderTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeAdherencePercentageReadModelReader>().For<IAdherencePercentageReadModelReader>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository, IRepository<IPerson>>();
		}

		public FakeAdherencePercentageReadModelReader Reader;
		public IAdherencePercentageViewModelBuilder Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeUserCulture Culture;
		public FakePersonRepository PersonRepository;
		
		[Test]
		public void ShouldBuildViewModel()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014,12,24)
			});
			
			var result = Target.Build(personId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBuildResultWith50PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014,12,24),
				TimeInAdherence = "74".Minutes(),
				TimeOutOfAdherence = "74".Minutes(),
				LastTimestamp = "2014-12-24 15:00".Utc(),
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldBuildResultWith100PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "12".Minutes(),
				TimeOutOfAdherence = "0".Minutes(),
				LastTimestamp = "2014-12-24 15:00".Utc(),
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldBuildResultWith0PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "0".Minutes(),
				TimeOutOfAdherence = "12".Minutes(),
				LastTimestamp = "2014-12-24 15:00".Utc(),
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldBuildResultWithNullWhenNoAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "0".Minutes(),
				TimeOutOfAdherence = "0".Minutes(),
				LastTimestamp = "2014-12-24 15:00".Utc(),
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldBuildEmptyResultWhenNoData()
		{
			var result = Target.Build(Guid.NewGuid());

			result.AdherencePercent.Should().Be.EqualTo(null);
			result.LastTimestamp.Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 11:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "0".Minutes(),
				TimeOutOfAdherence = "60".Minutes(),
				LastTimestamp = "2014-12-24 10:00".Utc(),
				IsLastTimeInAdherence = true
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 11:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "60".Minutes(),
				TimeOutOfAdherence = "0".Minutes(),
				LastTimestamp = "2014-12-24 10:00".Utc(),
				IsLastTimeInAdherence = false
			});
			
			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 11:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "60".Minutes(),
				TimeOutOfAdherence = "60".Minutes(),
				ShiftHasEnded = true,
			});

			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}


		[Test]
		public void ShouldUseCurrentTimeAsLastTimestamp()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Culture.IsCatalan();
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "1".Minutes(),
				LastTimestamp = "2014-12-24 14:00".Utc()
			});

			var result = Target.Build(personId);

			result.LastTimestamp.Should().Be("2014-12-24 15:00".Utc().AsCatalanShortTime());
		}

		[Test]
		public void ShouldAdjustTimestampToUserTimeZone()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 14:00");
			TimeZone.IsHawaii();
			Culture.IsCatalan();
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = "1".Minutes(),
				LastTimestamp = "2014-12-24 14:00".Utc()
			});

			var result = Target.Build(personId);

			result.LastTimestamp.Should().Be("2014-12-24 14:00".InHawaii().AsCatalanShortTime());
		}

		[Test]
		[Toggle(Toggles.RTA_CalculatePercentageInAgentTimezone_31236)]
		public void ShouldBuildForAgentsDate()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
			PersonRepository.Has(person);
			Now.Is("2015-02-24 09:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateTime(2015, 02, 23),
				TimeInAdherence = "1".Minutes(),
				LastTimestamp = "2015-02-24 09:00".Utc()
			});

			var result = Target.Build(person.Id.Value);

			result.AdherencePercent.Should().Not.Be(null);
		}

		[Test]
		[Toggle(Toggles.RTA_CalculatePercentageInAgentTimezone_31236)]
		public void ShouldSetAgentDateToUtcNowWhenPersonNotFound()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 15:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = new DateTime(2014, 12, 24),
			});

			var result = Target.Build(personId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAddTimeWhenCurrentlyInNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-12-24 11:00");
			Reader.Has(new AdherencePercentageReadModel
			{
				PersonId = personId,
				Date = "2014-12-24".Utc(),
				TimeInAdherence = "30".Minutes(),
				TimeOutOfAdherence = "30".Minutes(),
				LastTimestamp = "2014-12-24 10:00".Utc(),
				IsLastTimeInAdherence = null
			});

			var result = Target.Build(personId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

	}
}