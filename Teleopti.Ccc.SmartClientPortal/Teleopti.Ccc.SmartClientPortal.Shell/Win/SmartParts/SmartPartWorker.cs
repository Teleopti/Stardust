using System.Collections.Generic;
using System.Reflection;

namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// Represents a .
    /// </summary>
    public class SmartPartWorker
    {
        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        public GridWorkspace Workspace { get; set; }

        /// <summary>
        /// Holds the path to smartpart containing assemblies
        /// </summary>
        public string SmartPartAssemblyPath { get; set; }

        /// <summary>
        /// Shows the smart part.
        /// </summary>
        /// <param name="smartPartInfo">The smart part info.</param>
        /// <param name="parameters">The parameters.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        public void ShowSmartPart(SmartPartInformation smartPartInfo, IList<SmartPartParameter> parameters)
        {
            IList<SmartPartBase> loadedSmartPartCollection =
                Workspace.GetSmartPartByType(smartPartInfo.SmartPartName);

            if (loadedSmartPartCollection.Count == 0)
            {
                SmartPartBase smartpart = GetSmartPart(smartPartInfo);
                if (smartpart != null)
                {
                    smartpart.AddSmartPartParameter(parameters);
                    Workspace.Show(smartpart, smartPartInfo);
                }
            }
            else
            {
                foreach (SmartPartBase smartPartBase in loadedSmartPartCollection)
                {
                    if (smartPartBase.SmartPartId != smartPartInfo.SmartPartId)
                    {
                        SmartPartBase smartpart = GetSmartPart(smartPartInfo);
                        if (smartpart != null)
                        {
                            smartpart.AddSmartPartParameter(parameters);
                            Workspace.Show(smartpart, smartPartInfo);
                        }
                    }
                    else
                    {
                        smartPartBase.RefreshSmartPart(parameters);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartPartWorker"/> class.
        /// </summary>
        /// <param name="smartPartAssemblyPath">The smart part assembly path.</param>
        /// <param name="workspace">The workspace.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public SmartPartWorker(string smartPartAssemblyPath, GridWorkspace workspace)
        {
            SmartPartAssemblyPath = smartPartAssemblyPath;
            Workspace = workspace;
        }

        /// <summary>
        /// Gets the smart part.
        /// </summary>
        /// <param name="smartPartInfo">The smart part info.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        private static SmartPartBase GetSmartPart(SmartPartInformation smartPartInfo)
        {
            Assembly assembly = Assembly.Load(smartPartInfo.ContainingAssembly);
            SmartPartBase smartPart = assembly.CreateInstance(smartPartInfo.SmartPartName) as SmartPartBase;
            if (smartPart != null)
                smartPart.SmartPartId = smartPartInfo.SmartPartId;


            return smartPart;
        }
    }
}
