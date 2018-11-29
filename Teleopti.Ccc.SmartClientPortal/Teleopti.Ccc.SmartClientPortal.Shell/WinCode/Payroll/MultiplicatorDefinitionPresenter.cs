using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public class MultiplicatorDefinitionPresenter : CommonViewHolder<IMultiplicatorDefinitionViewModel>, IMultiplicatorDefinitionPresenter
    {
        private readonly IExplorerPresenter _explorerePresenter;
        private IMultiplicatorRepository _multiplicatorRepository;

        public MultiplicatorDefinitionPresenter(IExplorerPresenter presenter)
            : base(presenter)
        {
            _explorerePresenter = presenter;
        }

        public void LoadMultiplicatorDefinitions()
        {
            if (_explorerePresenter.Model.FilteredDefinitionSetCollection == null)
                return;

            ModelCollection.Clear();
            foreach (var definitionSet in _explorerePresenter.Model.FilteredDefinitionSetCollection)
            {   
                IList<MultiplicatorDefinition> weekMultiplicatorDefinitions = definitionSet.DefinitionCollection.OfType<MultiplicatorDefinition>().OrderBy(p => p.OrderIndex).ToList();
                foreach (var multiplicatorDefinition in weekMultiplicatorDefinitions)
                {
                    var multiplicatorType = definitionSet.MultiplicatorType;
                    var viewModel = new MultiplicatorDefinitionViewModel(multiplicatorDefinition);
                    viewModel.MultiplicatorCollection = ExplorerPresenter.Model.MultiplicatorCollection.Where(p => p.MultiplicatorType == multiplicatorType).ToList();
                    viewModel.MultiplicatorCollection.Insert(0, new Multiplicator(multiplicatorType) { Description = new Description(UserTexts.Resources.SelectAMultiplicator) });
                    ModelCollection.Add(viewModel);
                }
            }
        }

        private static DayOfWeekMultiplicatorDefinition getDefaultDayOfWeekMultiplicator(IMultiplicator multiplicator)
        {
            TimeSpan startTime = TimeSpan.FromHours(8);
            TimeSpan endTime = TimeSpan.FromHours(18);

            return new DayOfWeekMultiplicatorDefinition(multiplicator,
                                                        CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
                                                        new TimePeriod(startTime, endTime));
        }

        private static DateTimeMultiplicatorDefinition getDefaultDateTimeMultiplicator(IMultiplicator multiplicator)
        {
            DateOnly startDate = DateOnly.Today;
            DateOnly endDate = DateOnly.Today.AddDays(1);
            TimeSpan startTime = TimeSpan.FromHours(8);
            TimeSpan endTime = TimeSpan.FromHours(18);

            return new DateTimeMultiplicatorDefinition(multiplicator,startDate,endDate,startTime,endTime);
        }

        private IMultiplicator getAppropriateMultiplicator(IMultiplicatorDefinitionSet definitionSet)
        {
            foreach (IMultiplicator multiplicator in ExplorerPresenter.Model.MultiplicatorCollection)
            {
                if (multiplicator.MultiplicatorType == definitionSet.MultiplicatorType)
                {
                    return multiplicator;
                }
            }
            return null;
        }

        public int DeleteSelected(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition)
        {
            if (definitionSet.DefinitionCollection.Contains(multiplicatorDefinition))
            {
                int index =
                    ModelCollection.IndexOf(
                        ModelCollection.Where(p => p.DomainEntity == multiplicatorDefinition).SingleOrDefault());
                ModelCollection.RemoveAt(index);
                definitionSet.RemoveDefinition(multiplicatorDefinition);
                return index;
            }
            return -1;
        }

        public void AddNewAt(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinitionViewModel viewModelToCopy, int orderIndex, IList<IMultiplicatorDefinitionAdapter> multiplicatorDefinitionTypeCollection)
        {
            Type typeToInsert = viewModelToCopy.MultiplicatorDefinitionType.MultiplicatorDefinitionType;

            if (typeToInsert == typeof(DayOfWeekMultiplicatorDefinition))
            {
                // Day Of Week
                AddNewDayOfWeekAt(definitionSet, orderIndex, (IMultiplicator)viewModelToCopy.Multiplicator.Clone());
                IMultiplicatorDefinitionViewModel viewModel = ModelCollection[orderIndex];
                
                viewModel.DayOfWeek = viewModelToCopy.DayOfWeek;
                viewModel.StartTime = viewModelToCopy.StartTime;
                viewModel.EndTime = viewModelToCopy.EndTime;
            }
            else
            {
                // From - To
                AddNewDateTimeAt(definitionSet, orderIndex, (IMultiplicator)viewModelToCopy.Multiplicator.Clone());
                IMultiplicatorDefinitionViewModel viewModel = ModelCollection[orderIndex];
                viewModel.FromDate = viewModelToCopy.FromDate;
                viewModel.ToDate = viewModelToCopy.ToDate;
            }
        }

        public void AddNewDayOfWeek(IMultiplicatorDefinitionSet definitionSet)
        {
            if (definitionSet != null)
            {
                IMultiplicatorDefinition multiplicatorDefinition = getDefaultDayOfWeekMultiplicator(getAppropriateMultiplicator(definitionSet));
                definitionSet.AddDefinition(multiplicatorDefinition);
                ModelCollection.Add(createViewModel(multiplicatorDefinition, definitionSet.MultiplicatorType));
            }
        }

        public void AddNewDayOfWeekAt(IMultiplicatorDefinitionSet definitionSet, int orderIndex, IMultiplicator multiplicator)
        {
            if (definitionSet != null)
            {
                IMultiplicatorDefinition multiplicatorDefinition = getDefaultDayOfWeekMultiplicator(multiplicator);
                definitionSet.AddDefinitionAt(multiplicatorDefinition, orderIndex);
                ModelCollection.Insert(orderIndex,
                                       createViewModel(multiplicatorDefinition, definitionSet.MultiplicatorType));
            }
        }

        private IMultiplicatorDefinitionViewModel createViewModel(IMultiplicatorDefinition multiplicatorDefinition, MultiplicatorType multiplicatorType)
        {
            IMultiplicatorDefinitionViewModel viewModel = new MultiplicatorDefinitionViewModel(multiplicatorDefinition);
            viewModel.MultiplicatorCollection =
                ExplorerPresenter.Model.MultiplicatorCollection.Where(p => p.MultiplicatorType == multiplicatorType).ToList();
            viewModel.MultiplicatorCollection.Insert(0, new Multiplicator(multiplicatorType) { Description = new Description(UserTexts.Resources.SelectAMultiplicator) });

            return viewModel;
        }

        public void AddNewDateTime(IMultiplicatorDefinitionSet definitionSet)
        {
            if (definitionSet != null)
            {
                IMultiplicatorDefinition multiplicatorDefinition = getDefaultDateTimeMultiplicator(getAppropriateMultiplicator(definitionSet));
                definitionSet.AddDefinition(multiplicatorDefinition);
                
                ModelCollection.Add(createViewModel(multiplicatorDefinition, definitionSet.MultiplicatorType));                
            }
        }

        public void AddNewDateTimeAt(IMultiplicatorDefinitionSet definitionSet, int orderIndex, IMultiplicator multiplicator)
        {
            if (definitionSet != null)
            {
                IMultiplicatorDefinition multiplicatorDefinition = getDefaultDateTimeMultiplicator(multiplicator);
                definitionSet.AddDefinitionAt(multiplicatorDefinition, orderIndex);

                ModelCollection.Insert(orderIndex, createViewModel(multiplicatorDefinition, definitionSet.MultiplicatorType));
            }
        }

        public void MoveUp(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition)
        {
            if (definitionSet.DefinitionCollection.Contains(multiplicatorDefinition))
            {
                definitionSet.MoveDefinitionUp(multiplicatorDefinition);
                LoadMultiplicatorDefinitions();
            }
        }

        public void MoveDown(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition)
        {
            if (definitionSet.DefinitionCollection.Contains(multiplicatorDefinition))
            {
                definitionSet.MoveDefinitionDown(multiplicatorDefinition);
                LoadMultiplicatorDefinitions();
            }
        }

        public void Sort(SortingMode mode)
        {
            throw new NotImplementedException();
        }

        public void RefreshView()
        {
            LoadMultiplicatorDefinitions();
        }

        public string BuildCopyString(IList<IMultiplicatorDefinitionViewModel> viewModelCollection)
        {
            string returnValue = "";

            foreach (IMultiplicatorDefinitionViewModel viewModel in viewModelCollection)
            {
                string dayOfWeek = "";
                string startTime = "";
                string endTime = "";
                string fromDate = "";
                string toDate = "";

                if (viewModel.DayOfWeek.HasValue)
                    dayOfWeek = LanguageResourceHelper.TranslateEnumValue(viewModel.DayOfWeek.Value);
                if (viewModel.StartTime.HasValue)
                    startTime = viewModel.StartTime.Value.ToString();
                if (viewModel.EndTime.HasValue)
                    endTime = viewModel.EndTime.Value.ToString();
                if (viewModel.FromDate.HasValue)
                    fromDate = string.Format(CultureInfo.CurrentCulture, "{0} {1}",
                                             viewModel.FromDate.Value.ToShortDateString(),
                                             viewModel.FromDate.Value.ToShortTimeString());
                if (viewModel.ToDate.HasValue)
                    toDate = string.Format(CultureInfo.CurrentCulture, "{0} {1}",
                                             viewModel.ToDate.Value.ToShortDateString(),
                                             viewModel.ToDate.Value.ToShortTimeString());


                returnValue += string.Format(CultureInfo.CurrentCulture, "{0}", viewModel.MultiplicatorDefinitionType.Name);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}", dayOfWeek);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}", startTime);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}", endTime);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}", fromDate);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}", toDate);
                returnValue += string.Format(CultureInfo.CurrentCulture, "\t{0}\r\n", viewModel.Multiplicator.Description);
            }

            return returnValue;
        }

        public void UpdateMultiplicatorCollectionUponMultiplicatorChanges(EventMessageArgs e)
        {
            IMultiplicator multiplicator = MultiplicatorRepository.Get(e.Message.DomainObjectId);

            if (multiplicator != null)
            {
                switch (e.Message.DomainUpdateType)
                {
                    case DomainUpdateType.Insert:
                        ExplorerPresenter.Model.MultiplicatorCollection.Add(multiplicator);
                        break;
                    case DomainUpdateType.Update:
                        ExplorerPresenter.Model.MultiplicatorCollection.Remove(multiplicator);
                        ExplorerPresenter.Model.MultiplicatorCollection.Add(multiplicator);
                        break;
                    case DomainUpdateType.Delete:
                        ExplorerPresenter.Model.MultiplicatorCollection.Remove(multiplicator);
                        break;
                }
                RefreshView();
            }
        }

        private IMultiplicatorRepository MultiplicatorRepository
        {
            get
            {
                if (_multiplicatorRepository == null)
                    _multiplicatorRepository = new MultiplicatorRepository(_explorerePresenter.Helper.UnitOfWork);
                return _multiplicatorRepository;
            }
        }
    }
}
