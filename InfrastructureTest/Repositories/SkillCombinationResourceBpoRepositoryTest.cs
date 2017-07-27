using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillCombinationResourceBpoRepositoryTest
	{
		public ISkillCombinationResourceBpoRepository Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;

		[Test]
		public void ShouldPersistSingleBpoSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");

			Guid skillId = Guid.NewGuid();
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource()
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resource = 2.5,
					SkillCombination = new[] {skillId}
				}
			});

			var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 20, 1));
			SkillCombinationResourceWithCombinationId sk = (SkillCombinationResourceWithCombinationId) skillCombinationResources.First();
			
			var combinationResources = new List<SkillCombinationResourceBpo>
			{
				new SkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillCombinationId = sk.SkillCombinationId,
					Source = "TPBrazil"
				}
			};
			Target.PersistSkillCombinationResourceBpo(Now.UtcDateTime(), combinationResources);

			var loadedBpoCombinationResources = Target.LoadBpoSkillCombinationResources();
			loadedBpoCombinationResources.Count().Should().Be.EqualTo(1);
			var first = loadedBpoCombinationResources.First();
			first.SkillCombinationId.Should().Be.EqualTo(sk.SkillCombinationId);
			first.Resources.Should().Be.EqualTo(1);
			first.StartDateTime.Should().Be.EqualTo(startDate);
			first.EndDateTime.Should().Be.EqualTo(endDate);
			first.Source.Should().Be.EqualTo("TPBrazil");
		}

		[Test]
		public void ShouldPersistDifferentBposSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");

			Guid skillId = Guid.NewGuid();
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource()
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resource = 2.5,
					SkillCombination = new[] {skillId}
				}
			});

			var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 20, 1));
			SkillCombinationResourceWithCombinationId sk = (SkillCombinationResourceWithCombinationId)skillCombinationResources.First();

			var combinationResources = new List<SkillCombinationResourceBpo>
			{
				new SkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillCombinationId = sk.SkillCombinationId,
					Source = "TPBrazil"
				},
				new SkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillCombinationId = sk.SkillCombinationId,
					Source = "TPParis"
				}
			};
			Target.PersistSkillCombinationResourceBpo(Now.UtcDateTime(), combinationResources);

			var loadedBpoCombinationResources = Target.LoadBpoSkillCombinationResources();
			loadedBpoCombinationResources.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotAddSameBpoTwiceInSourceBpo()
		{
			Now.Is("2016-12-19 08:00");

			Guid skillId = Guid.NewGuid();
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource()
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resource = 2.5,
					SkillCombination = new[] {skillId}
				}
			});

			var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 20, 1));
			SkillCombinationResourceWithCombinationId sk = (SkillCombinationResourceWithCombinationId)skillCombinationResources.First();

			var combinationResources = new List<SkillCombinationResourceBpo>
			{
				new SkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillCombinationId = sk.SkillCombinationId,
					Source = "TPBrazil"
				},
				new SkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillCombinationId = sk.SkillCombinationId,
					Source = "TPBrazil"
				}
			};
			Target.PersistSkillCombinationResourceBpo(Now.UtcDateTime(), combinationResources);

			var bpoList = new Dictionary<Guid, string>();
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				bpoList= Target.LoadSourceBpo(connection);
			}

			bpoList.Count.Should().Be.EqualTo(1);
		}

	}
}