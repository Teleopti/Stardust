using System.Collections.Generic;
using System.Reflection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    public class SmartPartWorker
    {
        public GridWorkspace Workspace { get; set; }
		
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
		
        public SmartPartWorker(GridWorkspace workspace)
        {
            Workspace = workspace;
        }
		
        private static SmartPartBase GetSmartPart(SmartPartInformation smartPartInfo)
        {
            SmartPartBase smartPart = smartPartInfo.ContainingAssembly.CreateInstance(smartPartInfo.SmartPartName) as SmartPartBase;
            if (smartPart != null)
                smartPart.SmartPartId = smartPartInfo.SmartPartId;


            return smartPart;
        }
    }
}
