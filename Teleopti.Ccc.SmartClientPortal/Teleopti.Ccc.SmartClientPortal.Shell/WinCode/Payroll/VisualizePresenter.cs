using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public class VisualizePresenter : CommonViewHolder<VisualPayloadInfo>,IVisualizePresenter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizePresenter"/> class.
        /// </summary>
        /// <param name="explorerPresenter"></param>
        public VisualizePresenter(IExplorerPresenter explorerPresenter)
            : base(explorerPresenter)
        {
        }

        /// <summary>
        /// Loads the model.
        /// </summary>
        public void LoadModel(DateOnly selectedDate, TimeZoneInfo timeZoneInfo)
        {
            ModelCollection.Clear();
            if (ExplorerPresenter.Model.FilteredDefinitionSetCollection != null &&
                ExplorerPresenter.Model.FilteredDefinitionSetCollection.Count > 0)
            {
                int definitionSetCount = ExplorerPresenter.Model.FilteredDefinitionSetCollection.Count;
                for (int i = 0; i <= (definitionSetCount - 1); i++)
                {
                    IMultiplicatorDefinitionSet definitionSet = ExplorerPresenter.Model.FilteredDefinitionSetCollection[i];
                    IList<IMultiplicatorLayer> layers = definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(selectedDate.AddDays(-1), selectedDate.AddDays(1)), timeZoneInfo);

                    for (int j = 0; j <= (layers.Count - 1); j++)
                    {
                        IMultiplicatorLayer layer = layers[j];

                        VisualPayloadInfo payloadInfo = new VisualPayloadInfo(layer.Period.StartDateTimeLocal(timeZoneInfo),
                                                                              layer.Period.EndDateTimeLocal(timeZoneInfo),
                                                                              layer.Payload.DisplayColor,
                                                                              layer.Payload.Description.Name,
                                                                              layer.Payload.Description.ShortName,
                                                                              layer.Payload.MultiplicatorValue.ToString(CultureInfo.CurrentCulture),
                                                                              layer.LayerOriginalPeriod);
                        ModelCollection.Add(payloadInfo);

                    }
                    //Add dummy layer inorder to draw the projection layer.
                    ModelCollection.Insert(0,
                                           new VisualPayloadInfo(DateTime.Today, DateTime.Today,
                                                                 System.Drawing.Color.Empty,
                                                                 UserTexts.Resources.Projection,UserTexts.Resources.Projection, "0", TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(DateTime.Today,DateTime.Today,timeZoneInfo)));
                }
            }
        }
    }
}
