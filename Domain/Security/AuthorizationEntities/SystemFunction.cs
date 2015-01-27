using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class SystemFunction
	{
		private readonly IApplicationFunction _function;

		public SystemFunction(IApplicationFunction function, Func<IApplicationFunction, bool> isLicensed)
		{
			_function = function;

			IsLicensed = isLicensed(function);
			Hidden = function.IsPreliminary;
			ChildFunctions = function.ChildCollection.OfType<IApplicationFunction>().Select(f => new SystemFunction(f, isLicensed)).ToArray();
		}

		public IApplicationFunction Function
		{
			get { return _function; }
		}

		public SystemFunction FindByPath(string functionPath)
		{
			if (_function.FunctionPath == functionPath)
			{
				return this;
			}
			return ChildFunctions.Select(systemFunction => systemFunction.FindByPath(functionPath)).FirstOrDefault(foundFunction => foundFunction != null);
		}

		public ICollection<SystemFunction> ChildFunctions { get; private set; }

		public bool IsLicensed { get; private set; }
		public bool Hidden { get; private set; }

		public SystemFunction FindByForeignId(string foreignId)
		{
			if (_function.ForeignId == foreignId)
			{
				return this;
			}
			return ChildFunctions.Select(systemFunction => systemFunction.FindByForeignId(foreignId)).FirstOrDefault(foundFunction => foundFunction != null);
		}

		public void SetHidden()
		{
			Hidden = true;
		}
	}
}