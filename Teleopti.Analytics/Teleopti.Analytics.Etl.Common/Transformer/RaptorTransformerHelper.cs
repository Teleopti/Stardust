using System;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public static class RaptorTransformerHelper
	{
		public static IBusinessUnit CurrentBusinessUnit
		{
			get { return ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static DateTime GetUpdatedDate(IAggregateRoot aggregateRoot)
		{
			var root = aggregateRoot as IChangeInfo;
			if (root == null)
				return new DateTime(2059, 12, 31); //The root does not have CreatedOn/UpdatedOn propertys
			return root.UpdatedOn.Value;
		}

		public static void SetUpdatedOn(IAggregateRoot root, DateTime? updatedOn)
		{
			var rootCheck = root as IChangeInfo;
			if (rootCheck != null)
			{
				Type rootType = typeof(AggregateRoot);
				if (updatedOn.HasValue)
					rootType.GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, updatedOn);
			}
		}
	}
}