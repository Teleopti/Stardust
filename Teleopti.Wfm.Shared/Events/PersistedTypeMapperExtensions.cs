using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public static class PersistedTypeMapperExtensions
	{
		public static bool TypeForPersistedName(this PersistedTypeMapper typeMapper, string typeName, string assemblyName, out Type type)
		{
			if (assemblyName == null)
			{
				type = typeMapper.TypeForPersistedName(typeName);
				return true;
			}

			if (assemblyName.StartsWith("Teleopti."))
			{
				type = typeMapper.TypeForPersistedName($"{typeName}, {assemblyName}");
				return true;
			}

			type = null;
			return false;
		}

		public static bool TypeForPersistedName(this PersistedTypeMapper typeMapper, string persistedName, out Type type)
		{
			if (persistedName.IndexOfAny(new[] {',', '.'}) < 0)
			{
				type = typeMapper.TypeForPersistedName(persistedName);
				return true;
			}

			if (persistedName.StartsWith("Teleopti."))
			{
				var info = persistedName.Split(',');
				type = typeMapper.TypeForPersistedName($"{info[0]},{info[1]}");
				return true;
			}

			type = null;
			return false;
		}

		public static bool NameForPersistence(this PersistedTypeMapper typeMapper, Type type, out string persistedName)
		{
			if (type.Assembly.FullName.StartsWith("Teleopti."))
			{
				persistedName = typeMapper.NameForPersistence(type);
				return true;
			}

			persistedName = null;
			return false;
		}
	}
}