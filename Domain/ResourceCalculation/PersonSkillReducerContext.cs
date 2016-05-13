using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class PersonSkillReducerContext
	{
		[ThreadStatic]
		private static IPersonSkillReducer _reducer;

		public static IDisposable SetReducer(IPersonSkillReducer personSkillReducer)
		{
			_reducer = personSkillReducer;
			return new GenericDisposable(() => _reducer=null);
		}

		public static IPersonSkillReducer Fetch()
		{
			return _reducer ?? new NonDeletedPersonSkillReducer();
		}
	}
}