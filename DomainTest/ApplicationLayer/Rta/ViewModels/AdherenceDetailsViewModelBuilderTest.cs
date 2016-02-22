using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[TestFixture]
	[DomainTest]
	public class AdherenceDetailsViewModelBuilderTest : ISetup
	{
		public FakeAdherenceDetailsReadModelPersister Reader;
		public IAdherenceDetailsViewModelBuilder Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeUserCulture Culture;
		public FakePersonRepository PersonRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldBuildViewModel()
		{
			var personId = Guid.NewGuid();
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId
			});

			var result = Target.Build(personId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBuildSingleResultWith100PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(60),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(100);
		}

		[Test]
		public void ShouldBuildSingleResultWith50PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});

			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldBuildSingleResultWith0PercentAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(60),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldBuildSingleResultWithNullWhenNoAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						}
					}
				}
			});

			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(null);
		}

		[Test]
		public void ShouldBuildPercentAdherenceForEachActivity()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(60)
						},
						new ActivityAdherence
						{
							StartTime = "2014-11-20 9:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(0);
			result.Last().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldBuildEmptyResultWhenNoData()
		{
			var result = Target.Build(Guid.NewGuid());

			result.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldBuildResultForEachActivity()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.Zero,
							TimeOutOfAdherence = TimeSpan.Zero,
						}
					}
				}
			});

			var result = Target.Build(personId);

			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 8:30".Utc(),
					LastAdherence = true,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 8:30".Utc(),
					LastAdherence = false,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 10:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					ShiftEndTime = "2014-11-20 9:00".Utc(),
					LastUpdate = "2014-11-20 9:00".Utc(),
					LastAdherence = false,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterActivityHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 10:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 10:00".Utc(),
					LastAdherence = false,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						},
						new ActivityAdherence
						{
							StartTime = "2014-11-20 9:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(60),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						},
					}
				}
			});
			
			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldBuildWithActivityName()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Activities = new[]
					{
						new ActivityAdherence
						{
							Name = "Phone"
						}
					}
				}
			});

			var result = Target.Build(personId);
			result.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldFormatActivityTimes()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Culture.IsCatalan();
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							ActualStartTime = "2014-11-20 9:00".Utc(),
							StartTime = "2014-11-20 8:00".Utc(),
						}
					}
				}
			});
			
			var result = Target.Build(personId);
			result.First().StartTime.Should().Be("2014-11-20 8:00".Utc().ToShortTimeString(Culture));
			result.First().ActualStartTime.Should().Be("2014-11-20 9:00".Utc().ToShortTimeString(Culture));
		}

		[Test]
		public void ShouldBuildFooterItemIfShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Culture.IsCatalan();
			TimeZone.IsHawaii();
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					ShiftEndTime = "2014-11-20 9:00".Utc(),
					ActualEndTime = "2014-11-20 9:00".Utc(),
					LastUpdate = "2014-11-20 9:00".Utc(),
					LastAdherence = false,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc()
						}
					}
				}
			});
			
			var result = Target.Build(personId);

			result.Last().Name.Should().Be(UserTexts.Resources.End);
			result.Last().StartTime.Should().Be("2014-11-20 9:00".InHawaii().AsCatalanShortTime());
			result.Last().ActualStartTime.Should().Be("2014-11-20 9:00".InHawaii().AsCatalanShortTime());
			result.Last().AdherencePercent.Should().Be(null);
		}

		[Test]
		public void ShouldConvertActivityTimesToUserTimeZone()
		{
			var personId = Guid.NewGuid();
			TimeZone.IsHawaii();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							ActualStartTime = "2014-11-20 9:00".Utc(),
							StartTime = "2014-11-20 8:00".Utc(),
						}
					}
				}
			});

			var result = Target.Build(personId);

			result.First().StartTime.Should().Be("2014-11-20 8:00".InHawaii().AsShortTime(Culture));
			result.First().ActualStartTime.Should().Be("2014-11-20 9:00".InHawaii().AsShortTime(Culture));
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
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = person.Id.Value,
				Date = new DateTime(2015,02,23),
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence {}
					}
				}
			});

			var result = Target.Build(person.Id.Value);

			result.Should().Have.Count.GreaterThan(0);
		}


		[Test]
		public void ShouldNotReturnPercentageWhenOnlyNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 09:00".Utc(),
					LastAdherence = null,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = null,
							TimeOutOfAdherence = null,
						},
						new ActivityAdherence
						{
							StartTime = "2014-11-20 9:00".Utc()
						},
					}
				}
			});

			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(null);
		}

		[Test]
		public void ShouldReturn0PercentageWhenMostlyInNeutral()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 09:00".Utc(),
					LastAdherence = null,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = null,
							TimeOutOfAdherence = "5".Minutes(),
						},
						new ActivityAdherence
						{
							StartTime = "2014-11-20 9:00".Utc()
						},
					}
				}
			});

			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(0);
		}
		[Test]
		public void ShouldReturn100PercentageWhenMostlyInNeutral()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 09:00".Utc(),
					LastAdherence = null,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = "5".Minutes(),
							TimeOutOfAdherence = null,
						},
						new ActivityAdherence
						{
							StartTime = "2014-11-20 9:00".Utc()
						},
					}
				}
			});

			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(100);
		}

		[Test]
		public void ShouldNotConsiderNeutralAdherenceForOngoingActivity()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 10:00");
			Reader.Has(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 09:00".Utc(),
					LastAdherence = null,
					Activities = new[]
					{
						new ActivityAdherence
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = "30".Minutes(),
							TimeOutOfAdherence = "30".Minutes(),
						},
					}
				}
			});

			var result = Target.Build(personId);

			result.First().AdherencePercent.Should().Be(50);
		}

	}
}
