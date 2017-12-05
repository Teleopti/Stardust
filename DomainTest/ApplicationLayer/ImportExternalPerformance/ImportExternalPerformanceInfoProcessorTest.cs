﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ImportExternalPerformanceInfoProcessorTest : ISetup
	{
		public IExternalPerformanceInfoFileProcessor Target;
		public IStardustJobFeedback Feedback;
		public FakeExternalPerformanceRepository PerformanceRepository;
		public IPersonRepository PersonRepository;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ExternalPerformanceInfoFileProcessor>().For<IExternalPerformanceInfoFileProcessor>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
		}

		[Test]
		public void ShouldNotAllowOtherFileTypeThanCSV()
		{
			var fileData = new ImportFileData(){FileName = "test.xls"};
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.HasError.Should().Be.True();
			result.ErrorMessages.Should().Be.Equals(Resources.InvalidInput);
		}

		[Test]
		public void ShouldOnlyHave8FieldsForEachLine()
		{
			var invalidLine = "20171120,1,Kalle,Pettersson,Sales result,2,numeric,2000,extraline";
			var fileData = createFileData(invalidLine);

			var expectedErrorMsg = invalidLine + "," + string.Format(Resources.InvalidNumberOfFields, 8, 9);
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorMsg);
		}

		[Test]
		public void ShouldNotAllowInvalidDate()
		{
			var invalidDateRecord = "20172020,1,Kalle,Pettersson,Sales result,2,numeric,2000";
			var fileData = createFileData(invalidDateRecord);

			var expectedErrorMsg = invalidDateRecord + "," + Resources.ImportBpoWrongDateFormat;
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorMsg);
		}

		[Test]
		public void AgentIdShouldBeWithin100Characters()
		{
			string agentId = new String('a', 101);
			var invalidRecord =
				"20170820,"+ agentId + ",Kalle,Pettersson,Sales result,2,numeric,2000";
			var fileData = createFileData(invalidRecord);

			string errorMsg = Resources.AgentIdIsTooLong;
			var expectedErrorRecord = string.Format("{0},{1}", invalidRecord, errorMsg);
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void GameMeasureNameShouldBeWithin200Characters()
		{
			string measureName = new String('a', 201);
			var invalidRecord = $"20170820,1,Kalle,Pettersson,{measureName},2,numeric,2000";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.GameNameIsTooLong}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowWrongGameType()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,bla,87";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidGameType}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotParseScoreForInvalidNumericScore()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,numeric,InvalidScore";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidScore}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotParseScoreForInvalidPercentScore()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,percent,InvalidScore";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidScore}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowMoreThan10ExternalPerformancesCase1()
		{
			for (int i = 1; i < 11; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance(){ExternalId = i});
			}

			var the11thRecord = "20171120,1,Kalle,Pettersson,Quality Score,11,Percent,87";
			var fileData = createFileData(the11thRecord);

			var expectedErrorRecord = $"{the11thRecord},{Resources.OutOfMaximumLimit}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowMoreThan10ExternalPerformancesCase2()
		{
			setPerson(Guid.Empty);
			for (int i = 1; i < 10; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance(){ExternalId = i});
			}

			var the10thRecord = "20171120,1,Kalle,Pettersson,Quality Score,10,Percent,87";
			var the11thRecord = "20171120,1,Kalle,Pettersson,Quality Score,11,Percent,87";
			var records = new List<string>() {the10thRecord, the11thRecord};
			var fileData = createFileData(records);

			var expectedErrorRecord = $"{the11thRecord},{Resources.OutOfMaximumLimit}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].GameId.Should().Be.EqualTo(10);
			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldAllowExternalPerformancesWhenItWasExist()
		{
			setPerson(Guid.Empty);
			for (int i = 1; i < 11; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance() { ExternalId = i, DataType = ExternalPerformanceDataType.Percentage});
			}

			var theexistRecord = "20171120,1,Kalle,Pettersson,Quality Score,8,Percent,87";
			var fileData = createFileData(theexistRecord);

			var result = Target.Process(fileData, Feedback.SendProgress);

			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].GamePercentScore.Should().Be.EqualTo(new Percent(0.87));
		}

		[Test]
		public void ShouldNotAllowInvalidGameId()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,invalidId,numeric,87";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidGameId}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowWhenAgentDoNotExist()
		{
			var agentNotExistRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,numeric,87";
			var fileData = createFileData(agentNotExistRecord);

			var expectedErrorRecord = $"{agentNotExistRecord},{Resources.AgentDoNotExist}";
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldGetCorrectRecord()
		{
			var personId = Guid.NewGuid();
			setPerson(personId);
			var validRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,Percent,87";
			var fileData = createFileData(validRecord);

			var result = Target.Process(fileData, Feedback.SendProgress);

			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].DateFrom.Should().Be.EqualTo(new DateTime(2017, 11, 20));
			result.ValidRecords[0].GameName.Should().Be.EqualTo("Quality Score");
			result.ValidRecords[0].GameType.Should().Be.EqualTo("percent");
			result.ValidRecords[0].AgentId.Should().Be.EqualTo("1");
			result.ValidRecords[0].GameId.Should().Be.EqualTo(1);
			result.ValidRecords[0].GamePercentScore.Should().Be.EqualTo(new Percent(0.87));
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personId);
		}

		private void setPerson(Guid personId)
		{
			var person = new Person();
			person.SetEmploymentNumber("1");
			person.WithId(personId);
			PersonRepository.Add(person);
		}

		private ImportFileData createFileData(string record)
		{
			var records = new List<string> { record };

			var data = stringToArray(records);

			var fileData = new ImportFileData() { FileName = "test.csv", Data = data };
			return fileData;
		}

		private ImportFileData createFileData(IList<string> records)
		{
			var data = stringToArray(records);

			var fileData = new ImportFileData() { FileName = "test.csv", Data = data };
			return fileData;
		}

		private byte[] stringToArray(IList<string> lines)
		{
			byte[] data;

			using (MemoryStream ms = new MemoryStream())
			{
				using (StreamWriter sw = new StreamWriter(ms))
				{
					foreach (var line in lines)
					{
						sw.WriteLine(line);

					}
					sw.Close();
				}
				data = ms.ToArray();
			}

			return data;
		}
	}
}
