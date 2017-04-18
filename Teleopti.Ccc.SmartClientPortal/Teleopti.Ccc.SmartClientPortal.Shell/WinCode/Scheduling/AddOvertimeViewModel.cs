using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AddOvertimeViewModel : AddLayerViewModel<IActivity>, IAddOvertimeViewModel
    {
        private readonly ReadOnlyCollection<IMultiplicatorDefinitionSet> _definitionSets;

        public ICollectionView MultiplicatorDefinitionSet
        {
            get { return CollectionViewSource.GetDefaultView(_definitionSets) as ListCollectionView; }
        }

        public AddOvertimeViewModel(IEnumerable<IActivity> activities, IList<IMultiplicatorDefinitionSet> definitionSets, IActivity activity, ISetupDateTimePeriod period,TimeSpan interval)
            : base(activities, period, UserTexts.Resources.AddOvertime, interval)
        {
            _definitionSets = new ReadOnlyCollection<IMultiplicatorDefinitionSet>(definitionSets);
            if(_definitionSets.Count==0) CanOk = false;
            if (activity != null)
                Payloads.MoveCurrentTo(activity);
            else
                Payloads.MoveCurrentToFirst();
        }

        public IMultiplicatorDefinitionSet SelectedMultiplicatorDefinitionSet
        {
            get { return MultiplicatorDefinitionSet.CurrentItem as IMultiplicatorDefinitionSet; }

        }
    }
}
