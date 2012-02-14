using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Models
{
    /// <summary>
    /// Model class for the visualize grid view
    /// </summary>
    public class VisualizeViewModel : BaseModel, IVisualizeViewModel
    {
        private readonly ReadOnlyCollection<ReadOnlyCollection<VisualPayloadInfo>> _payloadInfo;
        private IList<ReadOnlyCollection<VisualPayloadInfo>> mainCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizeViewModel"/> class.
        /// </summary>
        /// <param name="ruleSetProjectionService"></param>
        /// <param name="ruleSet">The rule set.</param>
        public VisualizeViewModel(IRuleSetProjectionService ruleSetProjectionService, IWorkShiftRuleSet ruleSet) 
            : base(ruleSet)
        {
            var visualLayerCollectionList = ruleSetProjectionService.ProjectionCollection(ruleSet);
            mainCollection = new List<ReadOnlyCollection<VisualPayloadInfo>>();
            foreach (IVisualLayerCollection visualLayerCollection in visualLayerCollectionList)
            {
                IList<VisualPayloadInfo> payloadInfoList = new List<VisualPayloadInfo>();
                if(visualLayerCollection.HasLayers)
                {
                    foreach (IVisualLayer layer in visualLayerCollection)
                    {
                        VisualPayloadInfo payload = new VisualPayloadInfo(layer.Period.StartDateTime,
                                                                          layer.Period.EndDateTime,
                                                                          layer.DisplayColor(),
                                                                          layer.DisplayDescription().Name);
                        payloadInfoList.Add(payload);
                    }
                }
                mainCollection.Add(new ReadOnlyCollection<VisualPayloadInfo>(payloadInfoList));
            }
            _payloadInfo = new ReadOnlyCollection<ReadOnlyCollection<VisualPayloadInfo>>(mainCollection);
        }

        #region IVisualizeViewModel Members


        /// <summary>
        /// Gets the payload info.
        /// </summary>
        /// <value>The payload info.</value>
        public ReadOnlyCollection<ReadOnlyCollection<VisualPayloadInfo>> PayloadInfo
        {
            get
            {
                return _payloadInfo;
            }
        }

        #endregion
    }
}
