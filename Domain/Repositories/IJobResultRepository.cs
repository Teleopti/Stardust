using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IJobResultRepository : IRepository<IJobResult>
    {
        ICollection<IJobResult> LoadHistoryWithPaging(PagingDetail pagingDetail, params string[] jobCategories);
	    void AddDetailAndCheckSuccess(Guid jobResultId, IJobResultDetail detail, int expectedSuccessful);
	    IJobResult FindWithNoLock(Guid jobResultId);

	    IList<IJobResult> LoadAllWithNoLock();

    }
}