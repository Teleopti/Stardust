using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class CalculateAdherenceDetailsTest
	{
		[Test]
		public void ShouldReturnCalculatedResult()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResultIn100PercenWithOnlyInAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = now,
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(100);
		}

		[Test]
		public void ShouldResultIn80PercenWhenHaveInAndOutOfAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = now,
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldResultIn0PercenWhenOnlyOutOfAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateForTwoActivities()
		{
			var now = "2014-11-20 9:00".Utc();
			var personId = Guid.NewGuid();
			var date = "2014-11-20".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
				Model = new AdherenceDetailsModel
				{
					LastUpdate = now,
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(personId);

			result.First().AdherencePercent.Should().Be(0);
			result.Last().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldReturnEmptyResultIfNoDataIsFound()
		{
			var now = new ThisIsNow("2014-11-20 9:00".Utc());
			var personId = Guid.NewGuid();
			var agentDateProvider = MockRepository.GenerateStub<IAgentDateProvider>();
			agentDateProvider.Stub(x => agentDateProvider.Get(personId)).Return(new DateOnly(now.UtcDateTime()));

			var target = new CalculateAdherenceDetails(now,
				new FakeAdherenceDetailsReadModelReader(), new ThreadCulture(), new UtcTimeZone(), agentDateProvider);

			var result = target.ForDetails(Guid.NewGuid());

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnWhenActivityHasStartedEvenNoAdherenceData()
		{
			var now = "2014-11-20 9:00".Utc();
			var personId = Guid.NewGuid();
			var date = "2014-11-20".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotReturnWhenActivityHasNotStartedYet()
		{
			var now = "2014-11-20 9:00".Utc();
			var personId = Guid.NewGuid();
			var date = "2014-11-20".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldNotAddTimeAfterShiftHasEnded()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 10:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.First().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterActivityHasEnded()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 10:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new ThreadCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.First().AdherencePercent.Should().Be(50);
		}
		
		[Test]
		public void ShouldReturnModelWithFormattedTime()
		{
			var now = "2014-11-20 9:00".Utc();
			var detailModel = new AdherenceDetailModel
			{
				Name = "phone",
				TimeInAdherence = TimeSpan.FromMinutes(30),
				TimeOutOfAdherence = TimeSpan.FromMinutes(30),
				ActualStartTime = "2014-11-20 9:00".Utc(),
				StartTime = "2014-11-20 8:00".Utc(),
			};
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					LastUpdate = now,
					Details = new[]
					{
						detailModel
					}
				}
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
			new FakeAdherenceDetailsReadModelReader(new[] { model }), new SwedishCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);
			result.Single().Name.Should().Be(detailModel.Name);
			result.First().StartTime.Should().Be(detailModel.StartTime.Value.ToShortTimeString(new SwedishCulture().GetCulture()));
			result.First().ActualStartTime.Should().Be(detailModel.ActualStartTime.Value.ToShortTimeString(new SwedishCulture().GetCulture()));
			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldReturnEndStatusIfShiftHasEnded()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
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
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new SwedishCulture(), new UtcTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);

			result.Count().Should().Be(2);
			result.Last().Name.Should().Be(UserTexts.Resources.End);
			result.Last().StartTime.Should().Be(model.Model.ShiftEndTime.Value.ToShortTimeString(new SwedishCulture().GetCulture()));
			result.Last().ActualStartTime.Should().Be(model.Model.ActualEndTime.Value.ToShortTimeString(new SwedishCulture().GetCulture()));
			result.Last().AdherencePercent.Should().Be(null);
		}

		[Test]
		public void ShouldCalculateModelSpanOverUtcMidnight()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 8:00".Utc()
						}
					}
				}
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-21 02:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model }), new SwedishCulture(), new HawaiiTimeZone(), stubAgentDate(model));

			var result = target.ForDetails(model.PersonId);
			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldGetCorrectModel()
		{
			var model1 = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-20 18:00".Utc()
						}
					}
				}
			};
			var model2 = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-21".Utc(),
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							StartTime = "2014-11-21 08:00".Utc()
						}
					}
				}
			};

			var target1 = new CalculateAdherenceDetails(new ThisIsNow("2014-11-21 02:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model1 }), new SwedishCulture(), new HawaiiTimeZone(), stubAgentDate(model1));
			var result1 = target1.ForDetails(model1.PersonId);
			result1.Count().Should().Be(1);

			var target2 = new CalculateAdherenceDetails(new ThisIsNow("2014-11-21 17:00".Utc()),
				new FakeAdherenceDetailsReadModelReader(new[] { model2 }), new SwedishCulture(), new HawaiiTimeZone(), stubAgentDate(model2));
			var result2 = target2.ForDetails(model2.PersonId);
			result2.Count().Should().Be(1);
		}

		private static IAgentDateProvider stubAgentDate(AdherenceDetailsReadModel model)
		{
			var agentDateProvider = MockRepository.GenerateStub<IAgentDateProvider>();
			agentDateProvider.Stub(x => agentDateProvider.Get(model.PersonId)).Return(model.BelongsToDate);
			return agentDateProvider;
		}
	}
}
