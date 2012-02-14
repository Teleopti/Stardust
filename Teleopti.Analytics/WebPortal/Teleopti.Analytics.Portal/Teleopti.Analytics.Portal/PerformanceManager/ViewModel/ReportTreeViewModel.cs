using System.Collections.Generic;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.Portal.PerformanceManager.ViewModel
{
    public class ReportTreeViewModel
    {

        public ICollection<ReportTreeNodeViewModel> ReportTreeNodeViewModels
        {
            get
            {
                var list = new List<ReportTreeNodeViewModel>();
                var olapInformation = new OlapInformation();

                using (var clientProxy = new ClientProxy(olapInformation.OlapServer, olapInformation.OlapDatabase))
                {
                    CatalogItem[] catalogItems = clientProxy.ReportCollection();

                    foreach (var item in catalogItems)
                    {
                        list.Add(new ReportTreeNodeViewModel(item.Name.Trim(), item.Id.ToString()));
                    }    
                }

                return list;
            }
        }

    }

    public class ReportTreeNodeViewModel
    {
        public ReportTreeNodeViewModel(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Name { set; get; }
        public string Id { set; get; }
    }
}