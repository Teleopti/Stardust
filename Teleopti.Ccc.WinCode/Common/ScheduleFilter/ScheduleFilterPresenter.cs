using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using System.Drawing;


namespace Teleopti.Ccc.WinCode.Common.ScheduleFilter
{
    public class ScheduleFilterPresenter 
    {
        private IScheduleFilterView _view;
        private ScheduleFilterModel _model;
        IList<IPerson> _personWithGroup = new List<IPerson>();

        public ScheduleFilterPresenter(IScheduleFilterView view, ScheduleFilterModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _view.ButtonOkText(Resources.Ok);
            _view.ButtonCancelText(Resources.Cancel);
            _view.SetColor();
            _view.SetTexts();
            _view.AddTabPages(Resources.BusinessUnitHierarchy);
            InitializeMainView();
            InitializeTabPages();
            _model.CopyPersonsToOrgList(_model.SelectedPersons);
        }

        public IEnumerable<IPerson> PersonCollection
        {
            get { return _model.PersonCollection; }
            
        }

        public IBusinessUnit BusinessUnit
        {
            get { return _model.BusinessUnit; }
        }

        public DateOnly CurrentFilterDate
        {
            get { return _model.FilterDate; }
        }

        public void InitializeTabPages()
        {
            foreach (IGroupPage page in _model.GroupPages)
            {
                _view.AddTabPages(page);
            }
        }

        public void AddGroupingTreeStructure(IGroupPage page)
        {
            _personWithGroup.Clear();
            IList<IPerson> rootPersons = new List<IPerson>();
            foreach (IPerson person in _model.PersonCollection)
            {
                rootPersons.Add(person);
            }
            CccTreeNode rootNode = new CccTreeNode(page.Description.Name, new List<IPerson>(), false, 0);
            foreach (RootPersonGroup collection in page.RootGroupCollection)
            {
                IList<IPerson> persons = new List<IPerson>();
                CccTreeNode collectionNode = 
                    new CccTreeNode(collection.Description.Name, new List<IPerson>(), false, 2);
                collectionNode.Parent = rootNode;
                foreach (IPerson person in rootPersons)
                {
                    if (collection.PersonCollection.Contains(person))
                    {
                        persons.Add(person);
                        bool selected = _model.SelectedPersons.Contains(person);
                        CccTreeNode personNode = new CccTreeNode(_model.CommonAgentName(person), new List<IPerson> { person }, selected, 3);
                        collectionNode.Nodes.Add(personNode);
                        personNode.Parent = collectionNode;
                        ExpandParents(personNode);
                        AddPersonWithGroup(person);
                    }
                }
                rootNode.Nodes.Add(collectionNode);
                AddUnderGroups(collection, collectionNode);
            }

            foreach (IPerson person in _model.PersonCollection)
            {
                if (!_personWithGroup.Contains(person))
                {
                    CccTreeNode personNode = new CccTreeNode(_model.CommonAgentName(person), new List<IPerson> { person }, _model.SelectedPersons.Contains(person), 3);
                    rootNode.Nodes.Add(personNode);
                    ExpandParents(personNode);
                }
            }
            _view.CreateAndAddTreeNode(rootNode);
        }

        private void AddUnderGroups(PersonGroupBase groupCollection, CccTreeNode parentNode)
        {
            foreach (PersonGroupBase groupBase in groupCollection.ChildGroupCollection)
            {
                IList<IPerson> persons = new List<IPerson>();
                CccTreeNode childNode = new CccTreeNode(groupBase.Description.ToString(), new List<IPerson>(), false, 2);
                childNode.Parent = parentNode;
                foreach (IPerson person in _model.PersonCollection)
                {
                    if (groupBase.PersonCollection.Contains(person))
                    {
                        persons.Add(person);
                        CccTreeNode personNode = new CccTreeNode(_model.CommonAgentName(person), new List<IPerson> { person }, _model.SelectedPersons.Contains(person), 3);
                        AddPersonWithGroup(person);
                        childNode.Nodes.Add(personNode);
                        personNode.Parent = childNode;
                        ExpandParents(personNode);
                    }
                }
                parentNode.Nodes.Add(childNode);
                AddUnderGroups(groupBase, childNode);
            }
        }

        private void AddPersonWithGroup(IPerson personWithGroup)
        {
            _personWithGroup.Add(personWithGroup);
        }

        public void InitializeMainView()
        {
            CccTreeNode rootNode = new CccTreeNode(Resources.BusinessUnitHierarchy, null, false, 0);
            var hierarchyPeriod = _model.FilterPeriod;
            _model.SelectedPersonDictionary.Clear();

            foreach (ISite site in _model.BusinessUnit.SortedSiteCollection)
            {
                if (site.PersonsInHierarchy(_model.PersonCollection, hierarchyPeriod).Count > 0)
                {
                    CccTreeNode levelOneParentNode = new CccTreeNode(site.Description.Name, null, false, 1);
                    rootNode.Nodes.Add(levelOneParentNode);
                    levelOneParentNode.Parent = rootNode;

                    foreach (ITeam team in site.SortedTeamCollection)
                    {
                        if (team.PersonsInHierarchy(_model.PersonCollection, hierarchyPeriod).Count > 0)
                        {
                            CccTreeNode levelTwoParentNode = new CccTreeNode(team.Description.Name, null, false, 2);
                            levelOneParentNode.Nodes.Add(levelTwoParentNode);
                            levelTwoParentNode.Parent = levelOneParentNode;
                            foreach (IPerson person in team.PersonsInHierarchy(_model.PersonCollection, hierarchyPeriod))
                            {
                                TeamPersonKey key = new TeamPersonKey(team, person);
                                KeyValuePair<TeamPersonKey, IPerson> teamPersonKey = new KeyValuePair<TeamPersonKey, IPerson>(key,
                                                                                                              person);
                                CccTreeNode levelThreeParentNode = new CccTreeNode(_model.CommonAgentName(person), teamPersonKey, _model.SelectedPersons.Contains(person), 3);
                                levelTwoParentNode.Nodes.Add(levelThreeParentNode);
                                levelThreeParentNode.Parent = levelTwoParentNode;
                                ExpandParents(levelThreeParentNode);
                                if(_model.SelectedPersons.Contains(person))
                                    _model.SelectedPersonDictionary.Add(teamPersonKey);
                            }
                        }
                    }
                }
            }
            _view.CreateAndAddTreeNode(rootNode);
        }

        public static void ExpandParents(CccTreeNode cccTreeNode)
        {
            if (cccTreeNode.IsChecked)
            {
                CccTreeNode currentNode = cccTreeNode;
                while (currentNode.Parent != null)
                {
                    currentNode.Parent.DisplayExpanded = true;
                    currentNode = currentNode.Parent;
                }
            }
        }

        public void OnCancel()
        {
            _model.CopyPersonsFromOrgList();
            _view.CloseFilterForm();
        }
        public void OnCloseForm()
        {
            _view.CloseFilterForm();
        }

        public void OnMouseClick(Point point)
        {
            _view.OpenContextMenu(point);
        }

        public void OnToolStripMenuItemSearch()
        {
            _view.DisplaySearch();
        }
    
        public void OnTabPageSelectedIndexChanged(object tabTag) 
        {
            IGroupPage page = tabTag as IGroupPage;

            if (page != null)
            {
                _view.ClearSelectedTabControls();
                _model.TransferSelectedPersonDictionaryToSelectedPersons();
                AddGroupingTreeStructure(page);
                
            }
            else
            {
                _view.ClearSelectedTabControls();
                InitializeMainView();
            }
        }
    }
}
