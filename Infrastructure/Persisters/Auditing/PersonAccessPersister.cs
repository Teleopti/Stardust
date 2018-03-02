using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.Infrastructure.Persisters.Auditing
{
	public class PersonAccessPersister : IPersonAccessPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonAccessPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(PersonAccess model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
	INSERT INTO [Auditing].[PersonAccess]
           ([ActionPerformedBy]
           ,[Action]
           ,[ActionResult]
           ,[Data]
           ,[ActionPerformedOn]
           ,[Correlation])
     VALUES
           (:ActionBy
           ,:Action
           ,:ActionResult
           ,:ActionData
           ,:ActionOn
           ,:Correlation
)")
				.SetParameter("ActionBy", model.ActionPerformedBy)
				.SetParameter("Action", model.Action)
				.SetParameter("ActionResult", model.ActionResult)
				.SetParameter("ActionData", model.Data)
				.SetParameter("ActionOn", model.ActionPerformeOn)
				.SetParameter("Correlation", model.Correlation)
				.ExecuteUpdate();
		}
	}
}
