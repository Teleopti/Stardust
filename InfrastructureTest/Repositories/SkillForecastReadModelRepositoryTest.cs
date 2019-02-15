using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using List = Rhino.Mocks.Constraints.List;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillForecastReadModelRepositoryTest
	{
		public ISkillForecastReadModelRepository Target;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillTypeRepository SkillTypeRepository;
		public ISkillRepository SkillRepository;
		public IActivityRepository ActivityRepository;
		public SkillForecastSettingsReader SkillForecastSettingsReader;
		public IMutateNow Now;

		[Test]
		public void ShouldPersistSkillForecastIntervals()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();


			var skill = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var result = readIntervals();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldVerifyIfAllThevaluesAreSavedCorrectly()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();
			
			var skill = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15,
				AgentsWithShrinkage = 20,
				IsBackOffice = false,
				PercentAnswered = 0.8,
				AnsweredWithinSeconds = 10
			
			});
			Target.PersistSkillForecast(listOfIntervals);

			var skills = new[] { skill.Id.GetValueOrDefault() };
			var result = Target.LoadSkillForecast(skills, new DateTimePeriod(new DateTime(2019, 1, 23, 10, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 23, 11, 0, 0, DateTimeKind.Utc)));
			result.Count.Should().Be.EqualTo(1);
			result.First().Agents.Should().Be(10);
			result.First().AgentsWithShrinkage.Should().Be(20);
			result.First().Calls.Should().Be(400);
			result.First().SkillId.Should().Be(skill.Id.GetValueOrDefault());
			result.First().AverageHandleTime.Should().Be(15);
			result.First().IsBackOffice.Should().Be(false);
			result.First().StartDateTime.Should().Be(new DateTime(2019, 1, 23, 10, 0, 0));
			result.First().EndDateTime.Should().Be(new DateTime(2019, 1, 23, 10, 15, 0));
			result.First().PercentAnswered.Should().Be(0.8);
			result.First().AnsweredWithinSeconds.Should().Be(10);
		}

		[Test]
		public void ShouldReadIntervalsForTheRightSkill()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();

			var skill1 = SkillFactory.CreateMultisiteSkill("skill1", skillType, 15);
			var skill2 = SkillFactory.CreateMultisiteSkill("skill2", skillType, 15);
			skill1.Activity = activity;
			skill2.Activity = activity;
			SkillRepository.AddRange(new[] {skill1, skill2});
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill2.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var skills = new[] {skill1.Id.GetValueOrDefault()};
			var result =
				Target.LoadSkillForecast(skills, new DateTimePeriod(new DateTime(2019, 1, 23, 10, 0, 0,DateTimeKind.Utc), new DateTime(2019, 1, 23, 11, 0, 0,DateTimeKind.Utc)));
			result.Count.Should().Be.EqualTo(1);
			result.First().SkillId.Should().Be.EqualTo(skill1.Id.GetValueOrDefault());
		}


		[Test]
		public void ShouldReadIntervalsForTheSpecifiedPeriod()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();

			var skill1 = SkillFactory.CreateMultisiteSkill("skill1", skillType, 15);
			skill1.Activity = activity;
			SkillRepository.AddRange(new[] {skill1});
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 11, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 11, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var skills = new[] {skill1.Id.GetValueOrDefault()};
			var result =
				Target.LoadSkillForecast(skills, new DateTimePeriod(new DateTime(2019, 1, 23, 10, 0, 0,DateTimeKind.Utc), new DateTime(2019, 1, 23, 10, 30, 0,DateTimeKind.Utc)));
			result.Count.Should().Be.EqualTo(1);
			result.First().SkillId.Should().Be.EqualTo(skill1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldUpdateAlreadyPersistedSkillForecastIntervals()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();


			var skill = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);
			listOfIntervals.Clear();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 11, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 11, 15, 0),
				Agents = 20,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var result = readIntervals();
			result.Count.Should().Be.EqualTo(1);
			result.FirstOrDefault().Agents.Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldNotRemoveOtherSkillDaysOnUpdate()
		{
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();


			var skill1 = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			var skill2 = SkillFactory.CreateMultisiteSkill("skill2", skillType, 15);
			skill1.Activity = activity;
			skill2.Activity = activity;
			SkillRepository.AddRange(new[] { skill1, skill2 });
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 24, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 24, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});

			Target.PersistSkillForecast(listOfIntervals);
			listOfIntervals.Clear();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill2.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 24, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 24, 10, 15, 0),
				Agents = 20,
				Calls = 400,
				AverageHandleTime = 15
			});

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill1.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 25, 11, 15, 0),
				EndDateTime = new DateTime(2019, 1, 25, 11, 30, 0),
				Agents = 20,
				Calls = 400,
				AverageHandleTime = 15
			});

			Target.PersistSkillForecast(listOfIntervals);

			var result = readIntervals();
			result.Count.Should().Be.EqualTo(4);
			result.FirstOrDefault(s => s.StartDateTime == new DateTime(2019, 1, 24, 10, 0, 0))
				?.Agents.Should()
				.Be.EqualTo(20);
		}

		[Test]
		public void ShouldPurgeOldSkillForecasts()
		{
			var oldDate = new DateTime(2019, 1, 23, 10, 0, 0);
			Now.Is(oldDate);
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();


			var skill = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 23, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 23, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});

			listOfIntervals.Clear();
			Now.Is(oldDate.AddDays(SkillForecastSettingsReader.NumberOfDaysInPast));

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 31, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 31, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var result = readIntervals();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPurgeRelevantSkills()
		{
			var oldDate = new DateTime(2019, 1, 23, 10, 0, 0);
			Now.Is(oldDate);
			var listOfIntervals = new List<SkillForecast>();
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(skillType);
			var activity = new Activity("dummy activity");
			ActivityRepository.Add(activity);
			CurrentUnitOfWork.Current().PersistAll();


			var skill = SkillFactory.CreateMultisiteSkill("dummy", skillType, 15);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();


			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 24, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 24, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});

			Target.PersistSkillForecast(listOfIntervals);
			listOfIntervals.Clear();
			Now.Is(oldDate.AddDays(SkillForecastSettingsReader.NumberOfDaysInPast));

			listOfIntervals.Add(new SkillForecast()
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = new DateTime(2019, 1, 31, 10, 0, 0),
				EndDateTime = new DateTime(2019, 1, 31, 10, 15, 0),
				Agents = 10,
				Calls = 400,
				AverageHandleTime = 15
			});
			Target.PersistSkillForecast(listOfIntervals);


			var result = readIntervals();
			result.Count.Should().Be.EqualTo(2);
		}

		private List<SkillForecast> readIntervals()
		{
			var connection = new SqlConnection(InfraTestConfigReader.ConnectionString);
			connection.Open();
			var returnList = new List<SkillForecast>();
			using (var command =
				new SqlCommand(
					@"select [SkillId] ,[StartDateTime] ,[Agents] ,	[Calls] ,[AverageHandleTime]  from ReadModel.SkillForecast",
					connection))
			{
				//command.Parameters.AddWithValue("@bpoGuid", bpoGuid);
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					returnList.Add(new SkillForecast()
					{
						SkillId = reader.GetGuid(0),
						StartDateTime = reader.GetDateTime(1),
						Agents = reader.GetDouble(2),
						Calls = reader.GetDouble(3),
						AverageHandleTime = reader.GetDouble(4)
					});
				}
			}

			return returnList;
		}




	}
}