using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.SystemSetting
{
	//HACK until we remove serialization of settings to db
	//needed to support "old structure" when we had assembly win and wincode
	public class TypeConverter : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			//covers both old win and wincode
			var newAssemblyName = assemblyName.Contains("Teleopti.Ccc.Win")
				? "Teleopti.Ccc.SmartClientPortal.Shell"
				: assemblyName;

			var newTypeName = typeName
				.Replace("Teleopti.Ccc.WinCode.", "Teleopti.Ccc.SmartClientPortal.Shell.WinCode.")
				.Replace("Teleopti.Ccc.Win.", "Teleopti.Ccc.SmartClientPortal.Shell.Win.");

			return Type.GetType(newTypeName + ", " + newAssemblyName, true);
		}
	}
}