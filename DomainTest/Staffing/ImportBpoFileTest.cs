using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
		}

		[Test]
		public void ShouldReturnInformationOnInvalidFieldName()
		{
			var fileContents = @"sourceWrong,skillgroup,startdatetime,enddatetime,resources
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Single().Should().Contain("sourceWrong");
		}

		[Test]
		public void ShouldReturnInformationOnInvalidFieldNames()
		{
			var fileContents = @"sourceWrong,teleopti,startdatetime,enddatetime,resources
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5";

			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.Count.Should().Be.EqualTo(2);
			result.ErrorInformation.SingleOrDefault(e => e.Contains("sourceWrong")).Should().Not.Be.Null();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("teleopti")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationOnMissingSkill()
		{
			var fileContents = @"source,skillgroup,startdatetime,enddatetime,resources
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
			var fileContents = @"source,skillgroup,startdatetime,enddatetime,resources
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:00,2017-07-24 10:15,12.5
TPBRZIL,ChannelSales|Directsales,2017-07-24 10:15,2017-07-24 10:30,2.5
TPBRZIL,ChannelSales,2017-07-24 10:00,2017-07-24 10:15,8.75
TPBRZIL,ChannelSales,2017-07-24 10:15,2017-07-24 10:30,3.5
TPBRZIL,Directsales,2017-07-24 10:00,2017-07-24 10:15,1.5
TPBRZIL,Directsales,2017-07-24 10:15,2017-07-24 10:30,6.0";

			SkillRepository.Has("ChannelSales", new Activity());
			SkillRepository.Has("ChannelSales", new Activity());
			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
			result.ErrorInformation.SingleOrDefault(e => e.Contains("ChannelSales")).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnInformationIfAnSourceIsMissing()
		{
			var fileContents = @"source,skillgroup,startdatetime,enddatetime,resources
								Directsales,2017-07-24 10:15,2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnInformationIfStartDateTimeIsMissing()
		{
			var fileContents = @"source,skillgroup, startdatetime, enddatetime, resources
								TPBRZIL,Directsales,2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfItemsAreWithSpace()
		{
			var fileContents = @"source, skillgroup, startdatetime, enddatetime, resources
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfItemsAreMoreThanFive()
		{
			var fileContents = @"source, skillgroup, startdatetime, enddatetime, resources
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6,0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldSaveImportedDataInSkillCombinationBpo()
		{
			var fileContents = @"source, skillgroup, startdatetime, enddatetime, resources
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSaveImportedDataForTwoDifferentBpos()
		{
			var fileContents = @"source, skillgroup, startdatetime, enddatetime, resources
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0
								TPPARIS, Channel, 2017-07-24 10:15, 2017-07-24 10:30, 10.0";

			SkillRepository.Has("Directsales", new Activity());
			SkillRepository.Has("Channel", new Activity());
			var result = Target.ImportFile(fileContents, CultureInfo.InvariantCulture);
			result.Success.Should().Be.True();

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotSaveIfSkillIsMissing()
		{
			var fileContents = @"source, skillgroup, startdatetime, enddatetime, resources
								TPBRZIL, Directsales, 2017-07-24 10:15, 2017-07-24 10:30, 6.0
								TPBRZIL, KLINGON, 2017-07-24 10:15, 2017-07-24 10:30, 6.0";

			SkillRepository.Has("Directsales", new Activity());
			Target.ImportFile(fileContents, CultureInfo.InvariantCulture);

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(0);
		}
	}
}
