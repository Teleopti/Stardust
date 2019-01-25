using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class VisualizePresenter : BasePresenter<ReadOnlyCollection<VisualPayloadInfo>>, IVisualizePresenter
    {
        private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
    	private IList<int> _amountList;
        private IList<TimeSpan> _contractList;
	    

		public VisualizePresenter(IExplorerPresenter explorer, IDataHelper dataHelper, IRuleSetProjectionEntityService ruleSetProjectionEntityService)
            : base(explorer, dataHelper)
		{
			_ruleSetProjectionEntityService = ruleSetProjectionEntityService;
			
		}

	    public void LoadModelCollection(IWorkShiftAddCallback callback)
        {
            ClearModelCollection();
			Explorer.View.RefreshVisualizeView();
		    Explorer.View.RefreshActivityGridView();
			var model = Explorer.Model;
            var filteredRuleSetCollection = model.FilteredRuleSetCollection;
            if (filteredRuleSetCollection != null && filteredRuleSetCollection.Count > 0)
            {
                _amountList = new List<int>();
                _contractList = new List<TimeSpan>();
                TimeSpan startTime = TimeSpan.MaxValue, endTime = TimeSpan.Zero;
                var layerCollection = new List<ReadOnlyCollection<VisualPayloadInfo>>();
                foreach (var ruleSet in filteredRuleSetCollection)
                {
					if ((ruleSet.TemplateGenerator.StartPeriod.Period.StartTime < startTime))
						startTime = ruleSet.TemplateGenerator.StartPeriod.Period.StartTime;
					if ((ruleSet.TemplateGenerator.EndPeriod.Period.EndTime > endTime))
						endTime = ruleSet.TemplateGenerator.EndPeriod.Period.EndTime;
					if (callback != null)
						callback.StartNewRuleSet(ruleSet);
					var layers = getVisualLayers(_ruleSetProjectionEntityService.ProjectionCollection(ruleSet,callback));
					layerCollection.AddRange(layers);
					_amountList.Add(layers.Count);
					if (callback != null)
						callback.EndRuleSet();
                }
                SetModelCollection(new ReadOnlyCollection<ReadOnlyCollection<VisualPayloadInfo>>(layerCollection));

                TimeSpan difference;

                if (startTime.Minutes == 0)
                {
                    startTime = TimeSpan.FromHours(startTime.Hours - 1);
                    difference = endTime.Subtract(startTime);
                }
                else
                {
                    difference = endTime.Subtract(startTime);
                    difference = TimeSpan.FromHours((int)difference.TotalHours + 1);
                }

                model.SetShiftStartTime(startTime);
                model.SetShiftEndTime(endTime);

                if (difference.TotalHours>48)
                    model.SetVisualizeGridColumnCount(48 + 1);
                else
                    model.SetVisualizeGridColumnCount((int)(Math.Round(difference.TotalHours, MidpointRounding.AwayFromZero) + 1));
            }
        }

        public int GetNumberOfRowsToBeShown()
        {
            return ModelCollection.Count;
        }

        public IList<int> RuleSetAmounts()
        {
            return _amountList;
        }

        public IList<TimeSpan> ContractTimes()
        {
            return _contractList;
        }

        public void CopyWorkShiftToSessionDataClip(int rowIndex)
        {
            if (rowIndex < 1) return;
			var callback = new WorkShiftAddCallback();
            var list = new List<WorkShiftVisualLayerInfo>();
            foreach (IWorkShiftRuleSet ruleSet in Explorer.Model.FilteredRuleSetCollection)
            {
				var layers = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, callback);
                list.AddRange(layers);
            }

            if (list.Count > 0)
				ShiftInClip.Data = list[rowIndex - 1].WorkShift.EntityClone();
        }

        private IList<ReadOnlyCollection<VisualPayloadInfo>> getVisualLayers(IEnumerable<WorkShiftVisualLayerInfo> projections)
        {
            IList<ReadOnlyCollection<VisualPayloadInfo>>  mainCollection = new List<ReadOnlyCollection<VisualPayloadInfo>>();
                
            foreach (var layerInfo in projections)
            {
                var visualLayerCollection = layerInfo.VisualLayerCollection;
                _contractList.Add(visualLayerCollection.ContractTime());
                IList<VisualPayloadInfo> payloadInfoList = new List<VisualPayloadInfo>();
                if (visualLayerCollection.HasLayers)
                {
                    foreach (var layer in visualLayerCollection)
                    {
                        var payload = new VisualPayloadInfo(layer.Period.StartDateTime,
                                                                          layer.Period.EndDateTime,
                                                                          layer.Payload.ConfidentialDisplayColor_DONTUSE(null),
                                                                          layer.Payload.ConfidentialDescription_DONTUSE(null).Name);
                        var master = layer.Payload as IMasterActivity;
                        if(master != null)
                            payload.SetUnderlyingActivities(master.ActivityCollection);

                        payloadInfoList.Add(payload);
                    }
                }
                mainCollection.Add(new ReadOnlyCollection<VisualPayloadInfo>(payloadInfoList));
            }
            return mainCollection;
        }
    }
}
