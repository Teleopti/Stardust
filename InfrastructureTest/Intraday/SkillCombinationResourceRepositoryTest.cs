﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
    [AllTogglesOn]
	public class SkillCombinationResourceRepositoryTest
	{
		public ISkillCombinationResourceRepository Target;
		public MutableNow Now;
		
		[Test]
		public void ShouldPersistSingleSkillCombinationResource()
		{
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
			Target.PersistSkillCombinationResource(combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistSkillCombinationResource()
		{
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
			Target.PersistSkillCombinationResource(combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldLoadSkillCombinationInHexaDecimalOrder()
		{
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
			Target.PersistSkillCombinationResource(combinationResources);

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));

			var expectedSkillCombination = new[] {new Guid("3afb3e83-6d92-4196-9862-05694d2fa7d4"), new Guid("663ea425-c166-4f96-9b04-9d4a8cc36d63"), new Guid("f7001b28-b78a-481a-9849-7379bc56ed70")};
			loadedCombinationResources.Single().SkillCombination.SequenceEqual(expectedSkillCombination).Should().Be.True();
		}

		[Test]
		public void ShouldInsertDelta()
		{
			var skill = Guid.NewGuid();
			Now.Is("2016-06-16 08:00");
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
			Target.PersistSkillCombinationResource(combinationResources);

			Target.PersistChange(new SkillCombinationResource
			{
				SkillCombination = new[] { skill },
				StartDateTime = start,
				EndDateTime = end
			});

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldMergeDeltatWithResourceWithMultipleSkills()
		{
			var skill = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Now.Is("2016-06-16 08:00");
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
			Target.PersistSkillCombinationResource(combinationResources);

			Target.PersistChange(new SkillCombinationResource
			{
				SkillCombination = new[] { skill, skill2 },
				StartDateTime = start,
				EndDateTime = end
			});
			Target.PersistChange(new SkillCombinationResource
			{
				SkillCombination = new[] { skill, skill2 },
				StartDateTime = start,
				EndDateTime = end
			});

			var loadedCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}
	}

	
}
