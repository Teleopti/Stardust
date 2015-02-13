using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	[ReadModelTest]
	public class AdherenceDetailsReadModelPersisterTest
	{
		public IAdherenceDetailsReadModelPersister Target { get; set; }

		[Test]
		public void ShouldAdd()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					IsInAdherence = true,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							TimeInAdherence = "10".Minutes(),
							TimeOutOfAdherence = "20".Minutes(),
							ActualStartTime = "2014-11-19 8:05".Utc(),
							Name = "Phone",
							StartTime = "2014-11-19 8:00".Utc(),
							LastStateChangedTime = "2014-11-19 8:06".Utc()
						}
					}
				},
				State = new AdherenceDetailsReadModelState
				{
					Activities = new[] { new AdherenceDetailsReadModelActivityState() }
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.IsInAdherence.Should().Be(true);
			model.State.Activities.Should().Have.Count.EqualTo(1);
			var detail = model.Model.Details.First();
			detail.Name.Should().Be("Phone");
			detail.StartTime.Should().Be("2014-11-19 8:00".Utc());
			detail.ActualStartTime.Should().Be("2014-11-19 8:05".Utc());
			detail.TimeInAdherence.Should().Be("10".Minutes());
			detail.TimeOutOfAdherence.Should().Be("20".Minutes());
			detail.LastStateChangedTime.Should().Be( "2014-11-19 8:06".Utc());
		}

		[Test]
		public void ShouldUpdate()
		{
			var personId = Guid.NewGuid();
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					IsInAdherence = true,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							TimeInAdherence = "10".Minutes(),
							TimeOutOfAdherence = "20".Minutes(),
							ActualStartTime = "2014-11-19 8:05".Utc(),
							Name = "Phone",
							StartTime = "2014-11-19 8:00".Utc(),
							LastStateChangedTime = "2014-11-19 8:06".Utc()
						}
					}
				},
				State = new AdherenceDetailsReadModelState
				{
					Activities = new[] { new AdherenceDetailsReadModelActivityState() }
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.IsInAdherence.Should().Be(true);
			model.State.Activities.Should().Have.Count.EqualTo(1);
			var detail = model.Model.Details.First();
			detail.Name.Should().Be("Phone");
			detail.StartTime.Should().Be("2014-11-19 8:00".Utc());
			detail.ActualStartTime.Should().Be("2014-11-19 8:05".Utc());
			detail.TimeInAdherence.Should().Be("10".Minutes());
			detail.TimeOutOfAdherence.Should().Be("20".Minutes());
			detail.LastStateChangedTime.Should().Be("2014-11-19 8:06".Utc());
		}

		[Test]
		public void ShouldAddWithNulls()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							ActualStartTime = null,
							StartTime = null,
							LastStateChangedTime = null
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			var detail = model.Model.Details.First();
			detail.StartTime.Should().Be(null);
			detail.ActualStartTime.Should().Be(null);
			detail.LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldAddForEachDay()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
			});
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-20".Date(),
				PersonId = personId,
			});

			var model1 = Target.Get(personId, "2014-11-19".Date());
			model1.PersonId.Should().Be(personId);
			model1.BelongsToDate.Should().Be("2014-11-19".Date());

			var model2 = Target.Get(personId, "2014-11-20".Date());
			model2.PersonId.Should().Be(personId);
			model2.BelongsToDate.Should().Be("2014-11-20".Date());
		}
		
		[Test]
		public void ShouldUpdateWithNulls()
		{
			var personId = Guid.NewGuid();
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					IsInAdherence = true,
					Details = new[]
					{
						new AdherenceDetailModel
						{
							LastStateChangedTime = null,
							ActualStartTime = null
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Details.First().ActualStartTime.Should().Be(null);
			model.Model.Details.First().LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Add(new AdherenceDetailsReadModel{ PersonId = Guid.NewGuid(), Date = "2015-01-19".Utc()});

			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfThereIsNoData()
		{
			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldAddWithAlotOfData()
		{
			const string aVeryLongNameForActivty = "There are 4001 characters having fake ids 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a6388";
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							Name = aVeryLongNameForActivty
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Details.First().Name.Should().Be(aVeryLongNameForActivty);
		}

		[Test]
		public void ShouldUpdateWithAlotOfData()
		{
			const string aVeryLongNameForActivty = "There are 4001 characters having fake ids 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a6388";
			var personId = Guid.NewGuid();
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = "2014-11-19".Date(),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Details = new[]
					{
						new AdherenceDetailModel
						{
							Name = aVeryLongNameForActivty
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Details.First().Name.Should().Be(aVeryLongNameForActivty);
		}
	}

}
