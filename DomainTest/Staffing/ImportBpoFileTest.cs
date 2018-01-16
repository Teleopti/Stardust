using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Staffing 
{
	[DomainTest]
	public class ImportBpoFileTest : ISetup
	{
		public ImportBpoFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeUserTimeZone UserTimeZone;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnInformationOnInvalidFieldName()
		{
			var fileContents = @"sourceWrong,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Single().Should().Contain("sourceWrong");
		}

		[Test]
		public void ShouldReturnInformationOnInvalidFieldNames()
		{
			var fileContents = @"sourceWrong,teleopti,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Count.Should().Be.EqualTo(2);
			result.ErrorInformation.SingleOrDefault(e => e.Contains("sourceWrong")).Should().Not.Be.Null();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("teleopti")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptyFile()
		{
			var fileContents = @"";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Count.Should().Be.EqualTo(1);
			result.ErrorInformation.SingleOrDefault(e => e.Contains("1")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptyHeaderLine()
		{
			var fileContents = @"
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Count.Should().Be.EqualTo(1);
			result.ErrorInformation.SingleOrDefault(e => e.Contains("1")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSkipEmptyDataLine()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents

TPBRZIL,Channel Sales|Direct Sales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			SkillRepository.Has("Channel Sales", new Activity());
			SkillRepository.Has("Direct Sales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnInformationOnEmptySourceField()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("source")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptySkillGroupField()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("skillcombination")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptyStartDateTimeField()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("startdatetime")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptyEndDateTimeField()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,resources
TPBRZIL,ChannelSales,2017-07-24 10:00,,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("enddatetime")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnEmptyResourcesField()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("agents")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnAllEmptyFields()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
,,,,";
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Count.Should().Be.EqualTo(5);
			result.ErrorInformation.Count(e => e.Contains(" source")).Should().Be.EqualTo(1);
			result.ErrorInformation.Count(e => e.Contains(" skillcombination")).Should().Be.EqualTo(1);
			result.ErrorInformation.Count(e => e.Contains(" startdatetime")).Should().Be.EqualTo(1);
			result.ErrorInformation.Count(e => e.Contains(" enddatetime")).Should().Be.EqualTo(1);
			result.ErrorInformation.Count(e => e.Contains(" agents")).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnInformationOnMissingSkill()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:15,2017-07-24 10:30,2.5
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,8.75
TPBRZIL,ChannelSales,2017-07-24 10:15,2017-07-24 10:30,3.5
TPBRZIL,Directsales,2017-07-24 10:00,2017-07-24 10:15,1.5
TPBRZIL,Directsales,2017-07-24 10:15,2017-07-24 10:30,6.0";

			SkillRepository.Has("ChannelSales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("Directsales")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnDuplicateSkill()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,Directsales,2017-07-24 10:15,2017-07-24 10:30,6.0
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,8.75";

			SkillRepository.Has("ChannelSales", new Activity());
			SkillRepository.Has("ChannelSales", new Activity());
			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("ChannelSales")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationIfSourceIsMissing()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
								Directsales,2017-07-24 10:15,2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnInformationIfStartDateTimeIsMissing()
		{
			var fileContents = @"source,skillcombination, startdatetime, enddatetime, agents
								TPBRZIL,Directsales,2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfItemsUseInitialOrTrailingSpace()
		{
			var fileContents = @"source, skillcombination , startdatetime , enddatetime , agents 
								TPBRZIL, Directsales | Channel  , 2017-07-24 10:15 , 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			SkillRepository.Has("Channel", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldHandleSkillGroupsNamesWithSpace()
		{
			var fileContents = @"source, skillcombination , startdatetime , enddatetime , agents 
								TPBRZIL, Direct sales | Channel sales  , 2017-07-24 10:15 , 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Direct sales", new Activity());
			SkillRepository.Has("Channel sales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfItemsAreMoreThanFive()
		{
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6,0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldSaveImportedDataInSkillCombinationBpo()
		{
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSaveImportedDataForTwoDifferentBpos()
		{
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0
								TPPARIS, Channel, 2017-07-24 10:15, 2017-07-24 10:30, 10.0";

			SkillRepository.Has("Directsales", new Activity());
			SkillRepository.Has("Channel", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnFalseIfSkillIsMissing()
		{
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0
								TPBRZIL, KLINGON, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);

			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("KLINGON")).Should().Not.Be.Null();
			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnFalseIfStartDateTimeFormatIsWrong()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,2017-07-24 1000,2017-07-24 10:15,10.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("date")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnTrueIfImportingSameFileTwice()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,10.5";

			SkillRepository.Has("ChannelSales", new Activity());

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(1);

			var result2 = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result2.Success.Should().Be.True();
			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnFalseIfEndDateTimeFormatIsWrong()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 1015,10.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("date")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnFalseIfResourceFormatIsWrong()
		{
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,10.5€";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("resources")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnFalseIfDuplicateRecordsAreFoundInTheFile()
		{
			SkillRepository.Has("Channel Sales", new Activity());
			SkillRepository.Has("Direct Sales", new Activity());
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
TPBRZIL,Channel Sales|Direct Sales,2017-07-24 10:00,2017-07-24 10:15,10.5
TPBRZIL,Channel Sales|Direct Sales,2017-07-24 10:00,2017-07-24 10:15,10.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("Duplicate")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldVerifyThatObjectsAreNotSame()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var scr1 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Source = "BPO",
				Resources = 10,
				SkillIds = new List<Guid> { skill1  }
			};

			var scr2 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Source = "BPO",
				Resources = 10,
				SkillIds = new List<Guid> {  skill1, skill2 }
			};

			Assert.False(scr1.Equals(scr2));
		}

		[Test]
		public void ShouldVerifyThatObjectsAreSame()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var scr1 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Source = "BPO",
				Resources = 10,
				SkillIds =  new List<Guid> { skill1, skill2 }
			};

			var scr2 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Source = "BPO",
				Resources = 10,
				SkillIds = new List<Guid> { skill2, skill1 }
			};

			Assert.True(scr1.Equals(scr2));
		}

		[Test]
		public void ShouldVerifyThatObjectsAreNotSameWithEndDate()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var scr1 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0),
				Source = "BPO",
				Resources = 11,
				SkillIds = new List<Guid> { skill1, skill2 }
			};

			var scr2 = new ImportSkillCombinationResourceBpo
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Source = "BPO",
				Resources = 10,
				SkillIds = new List<Guid> { skill2, skill1 }
			};

			Assert.False(scr1.Equals(scr2));
		}

		[Test]
		public void ShouldConvertUserTimezoneToUtc()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			UserTimeZone.Is(timezone);
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			SkillRepository.Has("Channel", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.First().StartDateTime.Should().Be.EqualTo(new DateTime(2017, 7, 24, 8, 15, 0));
			skillCombResources.First().EndDateTime.Should().Be.EqualTo(new DateTime(2017, 7, 24, 8, 30, 0));
		}
		
		[Test]
		public void ShouldVerifyThatStartDateTimeAreEarlierThanEndDateTime()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			UserTimeZone.Is(timezone);
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:30, 2017-07-24 10:15, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("before")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldVerifyThatStartDateIsInTheFuture()
		{
			Now.Is("2017-07-24 10:30");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:29, 2017-07-24 10:45, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("future")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldVerifyThatAllSkillsHaveTheSameInterval()
		{
			Now.Is("2017-07-24 10:30");
			var fileContents = @"source,skillcombination,startdatetime,enddatetime,agents
								TPBRZIL, Directsales|Channelsales, 2017-07-24 10:30, 2017-07-24 10:45, 6.0";

			var channelSales = SkillFactory.CreateSkillWithId("Channelsales", 16);
			SkillRepository.Has("Directsales", new Activity());
			SkillRepository.Has(channelSales);

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("The imported resource interval length: 15 minutes differs from the skill: Channelsales default resolution: 16 minutes")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldVerifyThatImportIntervalsAreSameAsSkill()
		{
			Now.Is("2017-07-24 10:30");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:30, 2017-07-24 10:46, 6.0";

			var directSales = SkillFactory.CreateSkillWithId("Directsales", 15);
			SkillRepository.Has(directSales);
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("The imported resource interval length: 16 minutes differs from the skill: Directsales default resolution: 15 minutes")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldHandleSameSkillListedTwiceOnSameRow()
		{
			Now.Is("2017-07-24 10:00");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales|Directsales, 2017-07-24 10:30, 2017-07-24 10:45, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("The skill: Directsales is listed more than once on the same line.")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldVerifySourceStringLength()
		{
			Now.Is("2017-07-24 10:00");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								PBRZILsdkjfhdskjfjdfhkjsdhfkjhdskjfhsdkjfhkjdskjfkdshfkjsdhfkjshdfhskjdhfsdhfhsdkjfhkjdshfkjdshfkjaaa, Directsales, 2017-07-24 10:30, 2017-07-24 10:45, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("Source name is too long. Max 100 characters is allowed.")).Should().Not.Be.Null();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldHandleTeleoptiDateFormat()
		{
			Now.Is("2017-07-24 10:00");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 20170724 10:30, 20170724 10:45, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Not.Be.Empty();
			skillCombResources.First().StartDateTime.Should().Be(new DateTime(2017,07,24,10,30,00));
			skillCombResources.First().EndDateTime.Should().Be(new DateTime(2017,07,24,10,45,00));
		}
		
		[Test]
		public void ShouldHandleUsDateFormat()
		{
			Now.Is("2017-07-24 10:00");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 07/24/2017 10:30 AM, 07/24/2017 10:45 AM, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Not.Be.Empty();
		}
		
		[Test]
		public void ShouldHandleIsoDateFormat()
		{
			Now.Is("2017-07-24 10:00");
			var fileContents = @"source, skillcombination, startdatetime, enddatetime, agents
								TPBRZIL, Directsales, 2017-07-24 10:30, 2017-07-24 10:45, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
			
			var skillCombResources = SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo();
			skillCombResources.Should().Not.Be.Empty();
		}
	}
}
