using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillCombinationResourceRepositoryTest
	{
		public ISkillCombinationResourceRepository Target;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		
		[Test]
		public void ShouldPersistSingleSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {Guid.NewGuid()}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {Guid.NewGuid()}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(),combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldPersistSkillCombinationResourceButKeep8DaysHistoricalData()
		{
			var skillId = Guid.NewGuid();
			Now.Is("2017-06-01 08:00");
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			Now.Is("2017-06-09 08:00");
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldKeep8DaysHistoricalDataForDeltas()
		{
			var skillId = Guid.NewGuid();
			Now.Is("2017-06-01 08:00");
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1
				},
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1
				}
			});

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-09 08:00");
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] { skillId }
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1
				}
			});
			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(2);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
			loadedCombinationResources.Second().Resource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldLoadSkillCombinationInHexaDecimalOrder()
		{
			Now.Is("2016-12-19 08:00");
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {new Guid("f7001b28-b78a-481a-9849-7379bc56ed70"), new Guid("3afb3e83-6d92-4196-9862-05694d2fa7d4"), new Guid("663ea425-c166-4f96-9b04-9d4a8cc36d63")}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(),combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));

			var expectedSkillCombination = new[] {new Guid("3afb3e83-6d92-4196-9862-05694d2fa7d4"), new Guid("663ea425-c166-4f96-9b04-9d4a8cc36d63"), new Guid("f7001b28-b78a-481a-9849-7379bc56ed70")};
			loadedCombinationResources.Single().SkillCombination.SequenceEqual(expectedSkillCombination).Should().Be.True();
		}

		[Test]
		public void ShouldInsertDelta()
		{
			var skill = Guid.NewGuid();
			Now.Is("2016-12-19 08:00");
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);


			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 2,
					SkillCombination = new[] {skill}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(),combinationResources);

			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = 1
									  }
								  });

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(3d);
		}


		[Test]
		public void ShouldMergeDeltaWithPositiveAndNegativeValues()
		{
			var skill = Guid.NewGuid();
			Now.Is("2016-12-19 08:00");
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);


			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 2,
					SkillCombination = new[] {skill}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = 8
									  }
								  });
			Thread.Sleep(TimeSpan.FromSeconds(1));  //insertedOn is PK
			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = -3
									  }
								  });

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(7d);
		}

		[Test]
		public void ShouldAddEmptySkillCombinationResourceIfThereIsNoWhenAddingDelta()
		{
			var skill = Guid.NewGuid();
			Now.Is("2016-12-19 08:00");
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource  //to have skillCombination that we should always have
				{
					StartDateTime = start.AddHours(1),
					EndDateTime = end.AddHours(1),
					Resource = 1,
					SkillCombination = new[] {skill}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = 1
									  }
								  });

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldMergeDeltatWithResourceWithMultipleSkills()
		{
			var skill = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Now.Is("2016-12-19 08:00");
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);


			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(),combinationResources);

			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill, skill2},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = -1
									  }
								  });

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(2d);
		}

		[Test]
		public void ShouldMergeDeltatWithResourceWithMultipleSkillsInOneQuery()
		{
			var skill = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Now.Is("2016-12-19 08:00");
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				},
				new SkillCombinationResource
				{
					StartDateTime = start.AddMinutes(15),
					EndDateTime = end.AddMinutes(15),
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(),combinationResources);

			Target.PersistChanges(new []{
				new SkillCombinationResource
			{
				SkillCombination = new[] { skill, skill2 },
				StartDateTime = start,
				EndDateTime = end,
				Resource = -1
			}});
			Thread.Sleep(TimeSpan.FromSeconds(1));  //insertedOn is PK
			Target.PersistChanges(new[]
								  {
									  new SkillCombinationResource
									  {
										  SkillCombination = new[] {skill, skill2},
										  StartDateTime = start,
										  EndDateTime = end,
										  Resource = -1
									  }
								  });

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start)).Resource.Should().Be.EqualTo(1d);
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start.AddMinutes(15))).Resource.Should().Be.EqualTo(3d);
		}
	}

	
}
