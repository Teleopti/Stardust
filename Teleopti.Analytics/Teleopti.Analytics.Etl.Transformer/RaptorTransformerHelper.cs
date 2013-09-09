using System;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public static class RaptorTransformerHelper
    {
        public static IBusinessUnit CurrentBusinessUnit
        {
            get { return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static DateTime GetUpdatedDate(IAggregateRoot aggregateRoot)
        {
            IChangeInfo root = aggregateRoot as IChangeInfo;
            if(root==null)
                return new DateTime(2059, 12, 31); //The root does not have CreatedOn/UpdatedOn propertys
            return root.UpdatedOn.Value;
        }

        public static void SetCreatedOn(IAggregateRoot root, DateTime? createdOn)
        {
            IChangeInfo rootCheck = root as IChangeInfo;
            if (rootCheck != null)
            {
                Type rootType = typeof(AggregateRoot);
                if (createdOn.HasValue)
                    rootType.GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, createdOn);
            }
        }

        public static void SetUpdatedOn(IAggregateRoot root, DateTime? updatedOn)
        {
            IChangeInfo rootCheck = root as IChangeInfo;
            if (rootCheck != null)
            {
                Type rootType = typeof(AggregateRoot);
                if (updatedOn.HasValue)
                    rootType.GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, updatedOn);
            }
        }
    }
}