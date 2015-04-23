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
		}


		public void WillThrow(Exception exceptionToThrow)
		{
			_exceptionToThrow = exceptionToThrow;
		}
	}
}