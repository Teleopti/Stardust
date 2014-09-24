using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace CheckPreRequisites.Checks
{
	public static class FeatureChecker
	{
		

		public static FeatureState CheckAndEnable(string displayName, string feature)
		{
			var state = isAlreadyEnabled(displayName, feature);
			if (state.Enabled || state.NotInElevatedMood)
				return state;

			return EnableFeature(displayName, feature);
		}

		private static FeatureState isAlreadyEnabled(string displayName, string feature)
		{
			var ps = PowerShell.Create().AddCommand("dism");
			ps.AddArgument("/online");
			ps.AddArgument("/get-featureinfo");	
			ps.AddArgument("/featurename:" + feature);

			string output = ps.Invoke().Aggregate("", (current, result) => current + System.Environment.NewLine + result);
			if (output.Contains("elevated"))
				return new FeatureState { Enabled = false, NotInElevatedMood = true};

			if (output.Contains("State : Enabled"))
				return new FeatureState{ToolTip = output, Enabled = true, Status = "Enabled", DisplayName = displayName};

			return new FeatureState{Enabled = false};
		}

		private static FeatureState EnableFeature(string displayName,string feature)
		{
			PowerShell ps = PowerShell.Create().AddCommand("dism");
			ps.AddArgument("/online");
			ps.AddArgument("/enable-feature");
			ps.AddArgument("/featurename:" + feature);

			string output = ps.Invoke().Aggregate("", (current, result) => current + System.Environment.NewLine + result);
			if (output.Contains("successfully"))
				return new FeatureState { ToolTip = output, Enabled = true, Status = "Enabled", DisplayName = displayName };

			return new FeatureState { ToolTip = output, Enabled = false, Status = "Could not enabled feature!", DisplayName = displayName };
		}
	}

	public class FeatureState
	{
		public string Status { get; set; }
		public bool Enabled { get; set; }
		public string DisplayName { get; set; }
		public string ToolTip { get; set; }
		public bool NotInElevatedMood { get; set; }
	}

}
