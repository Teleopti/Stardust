using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public sealed class UnitOfWorkAttribute : AspectAttribute
	{
		public UnitOfWorkAttribute(DoCommit doCommit = DoCommit.WhenNoExceptionOccurs) : base(typeof(IUnitOfWorkAspect))
		{
			if (doCommit == DoCommit.No)
			{
				AspectType = typeof(IUnitOfWorkNoCommitAspect);				
			}
		}
	}
}