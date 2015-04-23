using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		private Exception _exceptionToThrow;
		
		public void Persist(PersonInfo personInfo)
		{
			if (_exceptionToThrow != null)
				throw _exceptionToThrow;
			LastPersist = personInfo;
		}

		public PersonInfo LastPersist { get; private set; }

		public void WillThrow(Exception exceptionToThrow)
		{
			_exceptionToThrow = exceptionToThrow;
		}
	}
}