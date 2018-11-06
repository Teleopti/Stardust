using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

	[TestFixture]
	public class StaffingAuditRepositoryTest : RepositoryTest<IStaffingAudit>
	{
		protected override IStaffingAudit CreateAggregateWithCorrectBusinessUnit()
		{
			return new StaffingAudit(LoggedOnPerson, StaffingAuditActionConstants.ImportBpo,"BPO","filename");
		}

		protected override void VerifyAggregateGraphProperties(IStaffingAudit loadedAggregateFromDatabase)
		{
			throw new NotImplementedException();
		}

		protected override Repository<IStaffingAudit> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			throw new NotImplementedException();
		}
	}
}
