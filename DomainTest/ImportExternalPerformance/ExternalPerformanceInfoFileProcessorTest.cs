using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ImportExternalPerformance
{
	[TestFixture]
	public class ExternalPerformanceInfoFileProcessorTest
	{
		private IExternalPerformanceRepository externalPerformanceRepository;
		private IPersonRepository personRepository;
		private ITenantLogonDataManager tenantLogonDataManager;

		private ExternalPerformanceInfoFileProcessor target;
		private IPerson person1, person2;

		[SetUp]
		public void SetUp()
		{
			externalPerformanceRepository = MockRepository.GenerateMock<IExternalPerformanceRepository>();
			externalPerformanceRepository.Stub(x => x.FindAllExternalPerformances()).Return(new List<IExternalPerformance>()
			{
				new ExternalPerformance()
			});

			person1 = PersonFactory.CreatePerson("person1");
			person1.SetId(Guid.NewGuid());
			person1.SetEmploymentNumber("emp-person1");

			person2 = PersonFactory.CreatePerson("person2");
			person2.SetId(Guid.NewGuid());
			person2.SetEmploymentNumber("emp-person2");

			personRepository = MockRepository.GenerateMock<IPersonRepository>();

			tenantLogonDataManager = MockRepository.GenerateMock<ITenantLogonDataManager>();
			target = new ExternalPerformanceInfoFileProcessor(externalPerformanceRepository, personRepository, tenantLogonDataManager);
		}

		[Test]
		public void ShouldNotProcessNonCsvFile()
		{
			var fileData = new ImportFileData
			{
				FileName = "abc.xls"
			};
			var errorMessages = new[] { Resources.InvalidInput };
			processFileData(fileData, true, Resources.InvalidInput, 0, 0, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWhenColumnCountIsIncorrect()
		{
			const string line = "1,2,3,4,5,6,7";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] {$"{line},{string.Format(Resources.InvalidNumberOfFields, 8, 7)}"};
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWhenDateIsInvalid()
		{
			const string line = "20170229,2,3,4,5,6,7,8";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.ImportBpoWrongDateFormat}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWhenGameNameIsTooLong()
		{
			var longGameName = "VeryLongGameName".PadRight(201, '0');
			var line = $"20171212,2,Ashley,Andeen,{longGameName},6,7,8";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.GameNameIsTooLong}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWithInvalidGameType()
		{
			const string line = "20171212,2,Ashley,Andeen,GameName,invalidGameType,7,8";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.InvalidGameType}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWithInvalidNumericGameScore()
		{
			const string line = "20171212,2,Ashley,Andeen,GameName,1,numeric,8..8";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.InvalidScore}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWithInvalidPercentGameScore()
		{
			const string line = "20171212,2,Ashley,Andeen,GameName,1,percent,a12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.InvalidScore}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWhenAgentIdentityTooLong()
		{
			var longAgentIdentity = "VeryLongIdentity".PadRight(131, '0');
			var line = $"20171212,{longAgentIdentity},Ashley,Andeen,GameName,1,numeric,12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.AgentIdIsTooLong}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldHandleErrorWithInvalidGameId()
		{
			const string line = "20171212,2,Ashley,Andeen,GameName,A2,numeric,12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new[] { $"{line},{Resources.InvalidGameId}" };
			processFileData(fileData, false, null, 0, 1, errorMessages);
		}

		[Test]
		public void ShouldFindPersonMatchEmploymentNumberFirst()
		{
			const string personIdentity = "identity1";
			var identies = new List<string> { personIdentity };
			personRepository.Stub(x => x.FindPersonByIdentities(identies)).IgnoreArguments()
				.Return(new List<PersonIdentityMatchResult>
				{
					new PersonIdentityMatchResult
					{
						Identity = personIdentity,
						MatchField = IdentityMatchField.EmploymentNumber,
						PersonId = person1.Id.Value
					},
					new PersonIdentityMatchResult
					{
						Identity = personIdentity,
						MatchField = IdentityMatchField.ExternalLogon,
						PersonId = person2.Id.Value
					},
				});

			var line = $"20171212,{personIdentity},Ashley,Andeen,GameName,2,numeric,12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new List<string>();
			var result = processFileData(fileData, false, null, 1, 0, errorMessages);
			var validRecord = result.ValidRecords.First();
			Assert.AreEqual(person1.Id.Value, validRecord.PersonId);
		}

		[Test]
		public void ShouldFindPersonMatchExternalLogon()
		{
			const string personIdentity = "identity1";
			var identies = new List<string> { personIdentity };
			personRepository.Stub(x => x.FindPersonByIdentities(identies)).IgnoreArguments()
				.Return(new List<PersonIdentityMatchResult>
				{
					new PersonIdentityMatchResult
					{
						Identity = personIdentity,
						MatchField = IdentityMatchField.ExternalLogon,
						PersonId = person2.Id.Value
					},
				});

			var line = $"20171212,{personIdentity},Ashley,Andeen,GameName,2,numeric,12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new List<string>();
			var result = processFileData(fileData, false, null, 1, 0, errorMessages);
			var validRecord = result.ValidRecords.First();
			Assert.AreEqual(person2.Id.Value, validRecord.PersonId);
		}

		[Test]
		public void ShouldFindPersonMatchApplicationLogonName()
		{
			const string personIdentity = "identity1";
			var identies = new List<string> { personIdentity };
			personRepository.Stub(x => x.FindPersonByIdentities(identies)).IgnoreArguments()
				.Return(new List<PersonIdentityMatchResult>());

			tenantLogonDataManager.Stub(x => x.GetLogonInfoForIdentities(new[] {personIdentity})).IgnoreArguments()
				.Return(new List<LogonInfoModel>
				{
					new LogonInfoModel
					{
						Identity = personIdentity,
						LogonName = "",
						PersonId = person2.Id.Value
					}
				});

			var line = $"20171212,{personIdentity},Ashley,Andeen,GameName,2,numeric,12";
			var fileData = new ImportFileData
			{
				Data = convertToBytes(line),
				FileName = "abc.csv"
			};
			var errorMessages = new List<string>();
			var result = processFileData(fileData, false, null, 1, 0, errorMessages);
			var validRecord = result.ValidRecords.First();
			Assert.AreEqual(person2.Id.Value, validRecord.PersonId);
		}

		private ExternalPerformanceInfoProcessResult processFileData(ImportFileData input, bool resultHasError,
			string expectedErrorMessage, int validRecordCount, int invalidRecordCount, IList<string> expectedMessages)
		{
			var messageBuilder = new StringBuilder();
			var result = target.Process(input, message => { messageBuilder.AppendLine(message); });

			Assert.AreEqual(resultHasError, result.HasError);
			Assert.AreEqual(expectedErrorMessage, result.ErrorMessages);
			Assert.AreEqual(validRecordCount, result.ValidRecords.Count);
			Assert.AreEqual(invalidRecordCount, result.InvalidRecords.Count);

			if (invalidRecordCount <= 0 || !expectedMessages.Any())
			{
				return result;
			}

			foreach (var msg in expectedMessages)
			{
				Assert.AreEqual(true, result.InvalidRecords.Any(x => x == msg));
			}

			return result;
		}

		private static byte[] convertToBytes(string lines)
		{
			return Encoding.UTF8.GetBytes(lines);
		}
	}
}
