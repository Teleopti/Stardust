using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	[IoCTest]
	public class CalculateAdherenceDetailsTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<FakeAdherenceDetailsReadModelReader>()
				.AsSelf()
				.As<IAdherenceDetailsReadModelReader>()
				.SingleInstance();
		}

		public FakeAdherenceDetailsReadModelReader Reader;
		public ICalculateAdherenceDetails Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeUserCulture Culture;

		[Test]
		public void ShouldReturnCalculatedResult()
		{
			var personId = Guid.NewGuid();
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 9:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(10)
						}
					}
				}
			});

			var result = Target.ForDetails(personId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResultIn100PercenWithOnlyInAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(60),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Single().AdherencePercent.Should().Be(100);
		}

		[Test]
		public void ShouldResultIn80PercenWhenHaveInAndOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});

			var result = Target.ForDetails(personId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldResultIn0PercenWhenOnlyOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(60),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateForTwoActivities()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(60)
						},
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 9:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.First().AdherencePercent.Should().Be(0);
			result.Last().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldReturnEmptyResultIfNoDataIsFound()
		{
			Now.Is("2014-11-20 9:00");
			
			var result = Target.ForDetails(Guid.NewGuid());

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnWhenActivityHasStartedEvenNoAdherenceData()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.Zero,
							TimeOutOfAdherence = TimeSpan.Zero
						}
					}
				}
			});

			var result = Target.ForDetails(personId);

			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotReturnWhenActivityHasNotStartedYet()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							TimeInAdherence = TimeSpan.Zero,
							TimeOutOfAdherence = TimeSpan.Zero
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 8:30".Utc(),
					LastAdherence = true,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 8:30".Utc(),
					LastAdherence = false,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(0),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldNotAddTimeAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 10:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					ShiftEndTime = "2014-11-20 9:00".Utc(),
					LastUpdate = "2014-11-20 9:00".Utc(),
					LastAdherence = false,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.First().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterActivityHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 10:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 10:00".Utc(),
					LastAdherence = false,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						},
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 9:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(60),
							TimeOutOfAdherence = TimeSpan.FromMinutes(0),
						},
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.First().AdherencePercent.Should().Be(50);
		}
		
		[Test]
		public void ShouldReturnModelWithFormattedTime()
		{
			var personId = Guid.NewGuid();
			Culture.IsCatalan();
			Now.Is("2014-11-20 9:00");
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-20 9:00".Utc(),
					Details = new[]
					{
						new AdherenceDetailModel
						{
							Name = "phone",
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
							ActualStartTime = "2014-11-20 9:00".Utc(),
							StartTime = "2014-11-20 8:00".Utc(),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);
			result.Single().Name.Should().Be("phone");
			result.First().StartTime.Should().Be("2014-11-20 8:00".Utc().ToShortTimeString(new CatalanCulture().GetCulture()));
			result.First().ActualStartTime.Should().Be("2014-11-20 9:00".Utc().ToShortTimeString(new CatalanCulture().GetCulture()));
			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldReturnEndStatusIfShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-20 9:00");
			Culture.IsCatalan();
			Reader.Data(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					ShiftEndTime = "2014-11-20 9:00".Utc(),
					ActualEndTime = "2014-11-20 9:00".Utc(),
					LastUpdate = "2014-11-20 9:00".Utc(),
					LastAdherence = false,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc(),
							TimeInAdherence = TimeSpan.FromMinutes(30),
							TimeOutOfAdherence = TimeSpan.FromMinutes(30),
						}
					}
				}
			});
			
			var result = Target.ForDetails(personId);

			result.Count().Should().Be(2);
			result.Last().Name.Should().Be(UserTexts.Resources.End);
			result.Last().StartTime.Should().Be("2014-11-20 9:00".Utc().ToShortTimeString(new CatalanCulture().GetCulture()));
			result.Last().ActualStartTime.Should().Be("2014-11-20 9:00".Utc().ToShortTimeString(new CatalanCulture().GetCulture()));
			result.Last().AdherencePercent.Should().Be(null);
		}
	}
}
