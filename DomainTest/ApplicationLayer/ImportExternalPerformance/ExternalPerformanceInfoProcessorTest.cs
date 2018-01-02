using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ExternalPerformanceInfoProcessorTest : ISetup
	{
		public IExternalPerformanceInfoFileProcessor Target;
		public FakeExternalPerformanceRepository PerformanceRepository;
		public FakePersonRepository PersonRepository;
		public FakeTenantPersonLogonQuerier TenantPersonLogonQuerier;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ExternalPerformanceInfoFileProcessor>().For<IExternalPerformanceInfoFileProcessor>();
			system.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			system.UseTestDouble<FakeTenantPersonLogonQuerier>().For<ITenantPersonLogonQuerier>();
		}

		[Test]
		public void ShouldAcceptOnlyCsvFile()
		{
			var fileData = new ImportFileData {FileName = "test.xls"};
			var result = Target.Process(fileData);

			result.HasError.Should().Be.True();
			result.ErrorMessages.Should().Be.Equals(Resources.InvalidInput);
		}

		[Test]
		public void ShouldOnlyHave8FieldsForEachLine()
		{
			const string invalidLine = "20171120,1,Kalle,Pettersson,Sales result,2,Numeric,2000,extraline";
			var fileData = createFileData(invalidLine);

			var expectedErrorMsg = invalidLine + "," + string.Format(Resources.InvalidNumberOfFields, 8, 9);
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorMsg);
		}

		[Test]
		public void ShouldNotAllowInvalidDate()
		{
			const string invalidDateRecord = "20172020,1,Kalle,Pettersson,Sales result,2,Numeric,2000";
			var fileData = createFileData(invalidDateRecord);

			var expectedErrorMsg = invalidDateRecord + "," + Resources.ImportBpoWrongDateFormat;
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorMsg);
		}

		[Test]
		public void ShouldRejectRecordIfAgentIdIsTooLong()
		{
			var agentId = new string('a', 131);
			var invalidRecord =
				"20170820,"+ agentId + ",Kalle,Pettersson,Sales result,2,Numeric,2000";
			var fileData = createFileData(invalidRecord);

			var errorMsg = Resources.AgentIdIsTooLong;
			var expectedErrorRecord = $"{invalidRecord},{errorMsg}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldRejectRecordIfExternalPerformanceTypeNameIsLongerThan200Characters()
		{
			var measureName = new String('a', 201);
			var invalidRecord = $"20170820,1,Kalle,Pettersson,{measureName},2,Numeric,2000";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.GameNameIsTooLong}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowInvalidGameType()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,bla,87";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.MeasureTypeMustBeEitherNumericOrPercent}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldRejectRecordIfScoreNumberIsInvalid()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,Numeric,InvalidScore";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidScore}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldRejectRecordIfScorePercentageIsInvalid()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,Percent,InvalidScore";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.InvalidScore}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowMoreThan10ExternalPerformanceTypesCase1()
		{
			for (var i = 1; i < 11; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance {ExternalId = i});
			}

			var the11thRecord = "20171120,1,Kalle,Pettersson,Quality Score,11,Percent,87";
			var fileData = createFileData(the11thRecord);

			var expectedErrorRecord = $"{the11thRecord},{string.Format(Resources.RowExceedsLimitOfGamificationMeasures, 10)}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldNotAllowMoreThan10ExternalPerformanceTypesCase2()
		{
			setPerson(Guid.Empty);
			for (var i = 1; i < 10; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance {ExternalId = i});
			}

			const string the10thRecord = "20171120,1,Kalle,Pettersson,Quality Score,10,Percent,87";
			const string the11thRecord = "20171120,1,Kalle,Pettersson,Quality Score,11,Percent,87";
			var records = new List<string> {the10thRecord, the11thRecord};
			var fileData = createFileData(records);

			var expectedErrorRecord = $"{the11thRecord},{string.Format(Resources.RowExceedsLimitOfGamificationMeasures, 10)}";
			var result = Target.Process(fileData);

			result.HasError.Should().Be.False();
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].GameId.Should().Be.EqualTo(10);
			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldAcceptRecordIfExternalPerformanceTypeAlreadyExists()
		{
			setPerson(Guid.Empty);
			for (var i = 1; i < 11; ++i)
			{
				PerformanceRepository.Add(new ExternalPerformance { ExternalId = i, DataType = ExternalPerformanceDataType.Percent});
			}

			var theExistRecord = "20171120,1,Kalle,Pettersson,Quality Score,8,Percent,87";
			var fileData = createFileData(theExistRecord);

			var result = Target.Process(fileData);

			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].GamePercentScore.Should().Be.EqualTo(new Percent(0.87));
		}

		[Test]
		public void ShouldNotAllowInvalidExternalPerformanceTypeId()
		{
			var invalidRecord = "20171120,1,Kalle,Pettersson,Quality Score,invalidId,Numeric,87";
			var fileData = createFileData(invalidRecord);

			var expectedErrorRecord = $"{invalidRecord},{Resources.MeasureIdMustContainAnInteger}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldFindAgentsByExternalLogon()
		{
			var per1 = Guid.NewGuid();
			var per2 = Guid.NewGuid();
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				PersonId = per1,
				ExternalLogonName = new List<string> { "externalLogon"}
			});
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				PersonId = per2,
				ExternalLogonName = new List<string> { "externalLogon"}
			});
			var agentsWith1ExternalLogonRecord = "20171120,externalLogon,Kalle,Pettersson,Quality Score,1,Numeric,87";
			var fileData = createFileData(agentsWith1ExternalLogonRecord);

			var result = Target.Process(fileData);
			result.ValidRecords.Count.Should().Be.EqualTo(2);
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(per1);
			result.ValidRecords[1].PersonId.Should().Be.EqualTo(per2);
		}

		[Test]
		public void ShouldReturnOnlyOneValidRecordWhenOnePersonIdMatchesTwoAgents()
		{
			PerformanceRepository.Add(new ExternalPerformance {ExternalId = 1, DataType = ExternalPerformanceDataType.Numeric});
			const string row = "20171120,1,John,Watson,Quality Score,1,Numeric,87";
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var person1 = new Person();
			person1.SetEmploymentNumber("1");
			person1.WithId(personId1);
			var person2 = new Person();
			person2.SetEmploymentNumber("2");
			person2.WithId(personId2);
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				ExternalLogonName = new List<string> {"1"},
				PersonId = personId2
			});
			var file = createFileData(row);

			var result = Target.Process(file);

			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personId1);
			result.InvalidRecords.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRejectRecordIfAgentDoesNotExist()
		{
			var agentNotExistRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,Numeric,87";
			var fileData = createFileData(agentNotExistRecord);

			var expectedErrorRecord = $"{agentNotExistRecord},{Resources.PersonIdCouldNotBeMatchedToAnyAgent}";
			var result = Target.Process(fileData);

			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorRecord);
		}

		[Test]
		public void ShouldGetCorrectRecordByEmployeeNumber()
		{
			var personId = Guid.NewGuid();
			setPerson(personId);
			var validRecord = "20171120,1,Kalle,Pettersson,Quality Score,1,Percent,87";
			var fileData = createFileData(validRecord);

			var result = Target.Process(fileData);

			result.HasError.Should().Be.False();
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].DateFrom.Should().Be.EqualTo(new DateTime(2017, 11, 20));
			result.ValidRecords[0].GameName.Should().Be.EqualTo("Quality Score");
			result.ValidRecords[0].GameType.Should().Be.EqualTo(ExternalPerformanceDataType.Percent);
			result.ValidRecords[0].AgentId.Should().Be.EqualTo("1");
			result.ValidRecords[0].GameId.Should().Be.EqualTo(1);
			result.ValidRecords[0].GamePercentScore.Should().Be.EqualTo(new Percent(0.87));
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldGetCorrectRecordByPersonInfo()
		{
			var personId = Guid.NewGuid();
			var tenantLogonName = "tenantAgent";
			var personInfoModel = new PersonInfoModel(){PersonId = personId, ApplicationLogonName = tenantLogonName };
			TenantPersonLogonQuerier.Add(personInfoModel);
			var validRecord = "20171120,tenantAgent,Kalle,Pettersson,Quality Score,1,Percent,87";
			var fileData = createFileData(validRecord);

			var result = Target.Process(fileData);

			result.HasError.Should().Be.False();
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].DateFrom.Should().Be.EqualTo(new DateTime(2017, 11, 20));
			result.ValidRecords[0].GameName.Should().Be.EqualTo("Quality Score");
			result.ValidRecords[0].GameType.Should().Be.EqualTo(ExternalPerformanceDataType.Percent);
			result.ValidRecords[0].AgentId.Should().Be.EqualTo(tenantLogonName);
			result.ValidRecords[0].GameId.Should().Be.EqualTo(1);
			result.ValidRecords[0].GamePercentScore.Should().Be.EqualTo(new Percent(0.87));
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldLookUpEmploymentNumberFirstWhenMatchingPersonIdAndAgent()
		{
			const string personid = "1";
			var row = $"20171120,{personid},x,x,Quality Score,1,Percent,87";
			var file = createFileData(row);

			var personid1 = Guid.NewGuid();
			var personid2 = Guid.NewGuid();
			var personid3 = Guid.NewGuid();

			var person1 = new Person();
			var person2 = new Person();
			var person3 = new Person();

			person1.WithId(personid1);
			person2.WithId(personid2);
			person3.WithId(personid3);

			person1.SetEmploymentNumber(personid);
			PersonRepository.Add(person1);

			PersonRepository.Add(person2);
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				ExternalLogonName = new List<string> {personid},
				PersonId = personid2
			});

			PersonRepository.Add(person3);
			TenantPersonLogonQuerier.Add(new PersonInfoModel
			{
				ApplicationLogonName = personid,
				PersonId = personid3
			});

			var result = Target.Process(file);
			result.InvalidRecords.Count.Should().Be.EqualTo(0);
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personid1);
		}

		[Test]
		public void ShouldLookUpExternalLogonIfEmploymentNumberHasNoMatchWhenMatchingPersonIdAndAgent()
		{
			const string personid = "1";
			var row = $"20171120,{personid},x,x,Quality Score,1,Percent,87";
			var file = createFileData(row);

			var personid1 = Guid.NewGuid();
			var personid2 = Guid.NewGuid();

			var person1 = new Person();
			var person2 = new Person();

			person1.WithId(personid1);
			person2.WithId(personid2);

			person1.SetEmploymentNumber(personid + "x");
			PersonRepository.Add(person1);

			PersonRepository.Add(person2);
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				ExternalLogonName = new List<string> { personid },
				PersonId = personid2
			});

			var result = Target.Process(file);
			result.InvalidRecords.Count.Should().Be.EqualTo(0);
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personid2);
		}

		[Test]
		public void ShouldLookUpExternalLogonBeforeApplicationLogonWhenMatchingPersonIdAndAgent()
		{
			const string personid = "1";
			var row = $"20171120,{personid},x,x,Quality Score,1,Percent,87";
			var file = createFileData(row);

			var personid1 = Guid.NewGuid();
			var personid2 = Guid.NewGuid();

			var person1 = new Person();
			var person2 = new Person();

			person1.WithId(personid1);
			person2.WithId(personid2);

			PersonRepository.Add(person1);
			PersonRepository.SetPersonExternalLogonInfo(new PersonExternalLogonInfo
			{
				ExternalLogonName = new List<string> { personid },
				PersonId = personid1
			});

			PersonRepository.Add(person2);
			TenantPersonLogonQuerier.Add(new PersonInfoModel
			{
				ApplicationLogonName = personid,
				PersonId = personid2
			});

			var result = Target.Process(file);
			result.InvalidRecords.Count.Should().Be.EqualTo(0);
			result.ValidRecords.Count.Should().Be.EqualTo(1);
			result.ValidRecords[0].PersonId.Should().Be.EqualTo(personid1);
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

			var fileData = new ImportFileData { FileName = "test.csv", Data = data };
			return fileData;
		}

		private ImportFileData createFileData(IList<string> records)
		{
			var data = stringToArray(records);

			var fileData = new ImportFileData { FileName = "test.csv", Data = data };
			return fileData;
		}

		private byte[] stringToArray(IList<string> lines)
		{
			byte[] data;

			using (var ms = new MemoryStream())
			{
				using (var sw = new StreamWriter(ms))
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
