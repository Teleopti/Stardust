﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture, Category("LongRunning")]
	[ReadModelUnitOfWorkTest]
	public class AdherenceDetailsReadModelPersisterTest
	{
		public IAdherenceDetailsReadModelPersister Target { get; set; }
		public IAdherenceDetailsReadModelReader Reader { get; set; }

		[Test]
		public void ShouldAdd()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014,11,19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-19 8:06".Utc(),
					LastAdherence = true,
					Activities = new[]
					{
						new ActivityAdherence
						{
							TimeInAdherence = "10".Minutes(),
							TimeOutOfAdherence = "20".Minutes(),
							ActualStartTime = "2014-11-19 8:05".Utc(),
							Name = "Phone",
							StartTime = "2014-11-19 8:00".Utc(),
						}
					}
				},
				State = new AdherenceDetailsReadModelState
				{
					Activities = new[] {new AdherenceDetailsReadModelActivityState()}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.LastAdherence.Should().Be(true);
			model.Model.LastUpdate.Should().Be("2014-11-19 8:06".Utc());
			model.State.Activities.Should().Have.Count.EqualTo(1);
			var detail = model.Model.Activities.First();
			detail.Name.Should().Be("Phone");
			detail.StartTime.Should().Be("2014-11-19 8:00".Utc());
			detail.ActualStartTime.Should().Be("2014-11-19 8:05".Utc());
			detail.TimeInAdherence.Should().Be("10".Minutes());
			detail.TimeOutOfAdherence.Should().Be("20".Minutes());
		}

		[Test]
		public void ShouldUpdate()
		{
			var personId = Guid.NewGuid();
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					LastUpdate = "2014-11-19 8:06".Utc(),
					LastAdherence = true,
					Activities = new[]
					{
						new ActivityAdherence
						{
							TimeInAdherence = "10".Minutes(),
							TimeOutOfAdherence = "20".Minutes(),
							ActualStartTime = "2014-11-19 8:05".Utc(),
							Name = "Phone",
							StartTime = "2014-11-19 8:00".Utc(),
						}
					}
				},
				State = new AdherenceDetailsReadModelState
				{
					Activities = new[] {new AdherenceDetailsReadModelActivityState()}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.LastAdherence.Should().Be(true);
			model.State.Activities.Should().Have.Count.EqualTo(1);
			model.Model.LastUpdate.Should().Be("2014-11-19 8:06".Utc());
			var detail = model.Model.Activities.First();
			detail.Name.Should().Be("Phone");
			detail.StartTime.Should().Be("2014-11-19 8:00".Utc());
			detail.ActualStartTime.Should().Be("2014-11-19 8:05".Utc());
			detail.TimeInAdherence.Should().Be("10".Minutes());
			detail.TimeOutOfAdherence.Should().Be("20".Minutes());
		}

		[Test]
		public void ShouldAddWithNulls()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					LastUpdate = null,
					Activities = new[]
					{
						new ActivityAdherence
						{
							ActualStartTime = null
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.LastUpdate.Should().Be(null);
			var detail = model.Model.Activities.First();
			detail.ActualStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldAddForEachDay()
		{
			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
			});
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 20),
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
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					LastUpdate = null,
					LastAdherence = true,
					Activities = new[]
					{
						new ActivityAdherence
						{
							ActualStartTime = null
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Activities.First().ActualStartTime.Should().Be(null);
			model.Model.LastUpdate.Should().Be(null);
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
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							Name = aVeryLongNameForActivty
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Activities.First().Name.Should().Be(aVeryLongNameForActivty);
		}

		[Test]
		public void ShouldUpdateWithAlotOfData()
		{
			const string aVeryLongNameForActivty = "There are 4001 characters having fake ids 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a6388";
			var personId = Guid.NewGuid();
			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
			});

			Target.Update(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							Name = aVeryLongNameForActivty
						}
					}
				}
			});

			var model = Target.Get(personId, "2014-11-19".Date());
			model.Model.Activities.First().Name.Should().Be(aVeryLongNameForActivty);
		}

		[Test]
		public void ShouldRead()
		{

			var personId = Guid.NewGuid();

			Target.Add(new AdherenceDetailsReadModel
			{
				Date = new DateTime(2014, 11, 19),
				PersonId = personId,
				Model = new AdherenceDetailsModel
				{
					Activities = new[]
					{
						new ActivityAdherence
						{
							TimeInAdherence = "10".Minutes()
						}
					}
				}
			});

			var model = Reader.Read(personId, "2014-11-19".Date());
			model.Model.Activities.Single().TimeInAdherence.Should().Be("10".Minutes());
		}
	}

}
