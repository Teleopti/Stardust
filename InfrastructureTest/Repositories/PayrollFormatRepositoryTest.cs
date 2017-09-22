using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PayrollFormatRepositoryTest: RepositoryTest<IPayrollFormat>
	{

		[Test]
		public void ShouldSaveToDb()
		{
			//<PayrollFormat Id="0e531434-a463-4ab6-8bf1-4696ddc9b296" Name="Teleopti Activities Export" DataSource="" />
  //<PayrollFormat Id="605b87c4-b98a-4fe1-9ea2-9b7308caa947" Name="Teleopti Detailed Export" DataSource="" />
  //<PayrollFormat Id="5a888bec-5954-466d-b245-639bfeda1bb5" Name="Teleopti Time Export" DataSource="" />
			var format2 = new PayrollFormat
			{
				Name = "Teleopti Detailed Export",
				FormatId = new Guid("605b87c4-b98a-4fe1-9ea2-9b7308caa947")
			};

			var rep = TestRepository(new ThisUnitOfWork(UnitOfWork));
			rep.Add(format2);
			Session.Flush();
			var all = rep.LoadAll();

			all.Count.Should().Be.EqualTo(1);
			all[0].Id.Should().Not.Be.EqualTo(null);
		}

		protected override IPayrollFormat CreateAggregateWithCorrectBusinessUnit()
		{
			var format1 = new PayrollFormat
			{
				Name = "Teleopti Activities Export",
				FormatId = new Guid("0e531434-a463-4ab6-8bf1-4696ddc9b296")
			};
			return format1;
		}

		protected override void VerifyAggregateGraphProperties(IPayrollFormat loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo("Teleopti Activities Export");
			loadedAggregateFromDatabase.FormatId.ToString().Should().Be.EqualTo("0e531434-a463-4ab6-8bf1-4696ddc9b296");
		}

		protected override Repository<IPayrollFormat> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PayrollFormatRepository(currentUnitOfWork);
		}
	}
}