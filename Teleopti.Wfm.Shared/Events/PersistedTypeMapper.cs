using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Teleopti.Ccc.Domain.Collection2;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class PersistedTypeMapper
	{
		private readonly Lazy<IDictionary<string, string>> persistedNameForTypeName;
		private readonly Lazy<IDictionary<string, string>> typeNameForPersistedName;

		public PersistedTypeMapper()
		{
			persistedNameForTypeName = new Lazy<IDictionary<string, string>>(makePersistedNameForTypeName);
			typeNameForPersistedName = new Lazy<IDictionary<string, string>>(makeTypeNameForPersistedName);
		}

		public string NameForPersistence(Type handlerType)
		{
			var typeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}";
			var persistedName = PersistedNameForTypeName(typeName);
			if (persistedName == null)
				throw new ArgumentException($"{typeName} is not mapped. {ExceptionInfoFor(typeName)}");
			return persistedName;
		}

		protected virtual string PersistedNameForTypeName(string typeName)
		{
			persistedNameForTypeName.Value.TryGetValue(typeName, out var persistedName);
			return persistedName;
		}

		protected virtual string ExceptionInfoFor(string typeName) => null;

		public Type TypeForPersistedName(string persistedName)
		{
			var typeName = TypeNameForPersistedName(persistedName);
			if (typeName == null)
				throw new ArgumentException($"{persistedName} is not mapped");
			return Type.GetType(typeName, true);
		}

		protected virtual string TypeNameForPersistedName(string persistedName)
		{
			typeNameForPersistedName.Value.TryGetValue(persistedName, out var typeName);
			return typeName;
		}

		private Dictionary<string, string> makePersistedNameForTypeName()
		{
			var dictionary = new Dictionary<string, string>();
			Mappings().ForEach(x =>
			{
				try
				{
					dictionary.Add(x.CurrentTypeName, x.CurrentPersistedName);
				}
				catch (ArgumentException e)
				{
					throw new ArgumentException($"{x.CurrentTypeName} seems to be a duplicate", e);
				}
			});
			return dictionary;
		}

		private Dictionary<string, string> makeTypeNameForPersistedName() =>
			(
				from m in Mappings()
				let allPersistedNames = new[] {m.CurrentPersistedName}.Concat(m.LegacyPersistedNames)
				from persistedName in allPersistedNames
				select new
				{
					persistedName,
					m.CurrentTypeName
				}
			)
			.ToDictionary(x => x.persistedName, x => x.CurrentTypeName);

		protected virtual IEnumerable<PersistedTypeMapping> Mappings()
		{
			return PersistedTypeMapperHandlerMappings.Mappings()
				.Concat(PersistedTypeMapperEventMappings.Mappings())
				.Concat(PersistedTypeMapperInfrastructureMappings.Mappings())
				;
		}
	}
}