using System;
using System.Collections.Generic;
using System.Drawing;
using Domain;
using Infrastructure;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverterTest.Properties;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Absence=Teleopti.Ccc.Domain.Scheduling.Absence;
using Activity=Teleopti.Ccc.Domain.Scheduling.Activity;
using IChangeInfo=Domain.IChangeInfo;
using Scenario=Teleopti.Ccc.Domain.Common.Scenario;
using ShiftCategory=Teleopti.Ccc.Domain.Scheduling.ShiftCategory;

namespace Teleopti.Ccc.DatabaseConverterTest.Helpers
{
    /// <summary>
    /// Helpers
    /// </summary>
    public class AgentDayFactory
    {
        private static string longString60 = Resources.LongString60chars;
        //global::Domain.AgentDay _agDay;

        private ObjectPairCollection<Agent, IPerson> _agentPairList =
            new ObjectPairCollection<Agent, IPerson>();

        private ObjectPairCollection<global::Domain.Activity, IActivity> _actPairList =
            new ObjectPairCollection<global::Domain.Activity, IActivity>();

        private ObjectPairCollection<global::Domain.Absence, IAbsence> _absPairList =
            new ObjectPairCollection<global::Domain.Absence, IAbsence>();

        private ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> _shiftCatPairList =
            new ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory>();

        private ObjectPairCollection<global::Domain.Scenario, IScenario> _scenarioPairList =
            new ObjectPairCollection<global::Domain.Scenario, IScenario>();

        private ObjectPairCollection<global::Domain.Absence, global::Domain.Activity> _absenceActivityList =
            new ObjectPairCollection<global::Domain.Absence, global::Domain.Activity>();


        private global::Domain.Activity _oldAct1;
        private global::Domain.Activity _oldAct2;
        private global::Domain.Activity _oldAct3;

        private global::Domain.Absence _oldAbs1;
        private global::Domain.Absence _oldAbs2;

        private Absence _newAbs1;
        private Absence _newAbs2;

        private Activity _newAct1;
        private Activity _newAct2;

        private Agent _oldAgent;

        private SchedType _schedType;

        private global::Domain.ShiftCategory _oldShiftCat1;
        private global::Domain.ShiftCategory _oldShiftCat2;
        private global::Domain.ShiftCategory _oldShiftCat3;

        private ShiftCategory _newShiftCat1;
        private ShiftCategory _newShiftCat2;
        private ShiftCategory _newShiftCat3;

        private global::Domain.Scenario _oldScenario1;
        private global::Domain.Scenario _oldScenario2;

        private global::Domain.Scenario _oldScenario3;

        private Scenario _newScenario1;

        private Scenario _newScenario2;

        private Scenario _newScenario3;


        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDayFactory"/> class.
        /// </summary>
        public AgentDayFactory()
        {
            //TODO: ROK, Ändrat till substring 50 - varför finns den långa varianten? Har lagt in längdbegränsing i konvertering av skiftkategorier

            //TODO: Should be 50 for Short Name? need further impl. in description object!
            Setup();
            _newShiftCat3.Description = new Description(_newShiftCat3.Description.Name,("T" + longString60).Substring(0, 25));
            _newShiftCat3.DisplayColor = Color.AliceBlue;
            _actPairList.Add(_oldAct1, _newAct1);
            _actPairList.Add(_oldAct2, _newAct2);
            _shiftCatPairList.Add(_oldShiftCat1, _newShiftCat1);
            _shiftCatPairList.Add(_oldShiftCat2, _newShiftCat2);
            _shiftCatPairList.Add(_oldShiftCat3, _newShiftCat3);
            _scenarioPairList.Add(_oldScenario1, _newScenario1);
            _scenarioPairList.Add(_oldScenario2, _newScenario2);
            _scenarioPairList.Add(_oldScenario3, _newScenario3);
            _absPairList.Add(_oldAbs1, _newAbs1);
            _absPairList.Add(_oldAbs2, _newAbs2);

            Domain.Common.Person newAgent = new Domain.Common.Person();
            //todo: RK remmat - vad ska denna finnas för?
            // newAgent.Name = "Kalle" +longString60;
            _agentPairList.Add(_oldAgent, newAgent);
        }

        /// <summary>
        /// Gets the type of the sched.
        /// </summary>
        /// <value>The type of the sched.</value>
        public SchedType ScheduleType
        {
            get { return _schedType; }
        }

        /// <summary>
        /// Gets the agent pair list.
        /// </summary>
        /// <value>The agent pair list.</value>
        public ObjectPairCollection<Agent, IPerson> AgentPairList
        {
            get { return _agentPairList; }
        }

        /// <summary>
        /// Gets the absence pair list.
        /// </summary>
        /// <value>The agent pair list.</value>
        public ObjectPairCollection<global::Domain.Absence, IAbsence> AbsPairList
        {
            get { return _absPairList; }
        }

        /// <summary>
        /// Gets the act pair list.
        /// </summary>
        /// <value>The act pair list.</value>
        public ObjectPairCollection<global::Domain.Activity, IActivity> ActPairList
        {
            get { return _actPairList; }
        }

        /// <summary>
        /// Gets the shiftCat pair list.
        /// </summary>
        /// <value>The shiftCat pair list.</value>
        public ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> ShiftCatPairList
        {
            get { return _shiftCatPairList; }
        }

        /// <summary>
        /// Gets the scenario pair list.
        /// </summary>
        /// <value>The scenario pair list.</value>
        public ObjectPairCollection<global::Domain.Scenario, IScenario> ScenarioPairList
        {
            get { return _scenarioPairList; }
            set { _scenarioPairList = value; }
        }

        public ObjectPairCollection<global::Domain.Absence, global::Domain.Activity> AbsenceActivityList
        {
            get { return _absenceActivityList; }
        }

        /// <summary>
        /// Returns an agent day.
        /// </summary>
        /// <returns></returns>
        public AgentDay AgentDay()
        {
            DateTime schedDate = new DateTime(2007, 1, 1);
            ICccListCollection<FillupShift> fillUpList = new CccListCollection<FillupShift>();
            fillUpList.FinishReadingFromDatabase(CollectionType.Locked);

            IntegerDateKey unique = new IntegerDateKey(_oldAgent.Id, schedDate);
            byte[] stamp = {1};
            IChangeInfo changeInfo = new DatasourceChangeInfo();
            AgentAssignment agAss = new AgentAssignment(null, fillUpList, stamp, null, unique, changeInfo,null);
            //global::Domain.Scenario scenario = new global::Domain.Scenario(-1, "Scenario" + longString60, true, false);

            return new AgentDay(schedDate, _oldAgent, _oldScenario1, agAss, "Note" + longString60);
        }
        public AgentDay AgentDayWithoutNote()
        {
            DateTime schedDate = new DateTime(2007, 1, 1);
            ICccListCollection<FillupShift> fillUpList = new CccListCollection<FillupShift>();
            fillUpList.FinishReadingFromDatabase(CollectionType.Locked);

            IntegerDateKey unique = new IntegerDateKey(_oldAgent.Id, schedDate);
            byte[] stamp = { 1 };
            IChangeInfo changeInfo = new DatasourceChangeInfo();
            AgentAssignment agAss = new AgentAssignment(null, fillUpList, stamp, null, unique, changeInfo, null);

            return new AgentDay(schedDate, _oldAgent, _oldScenario1, agAss, string.Empty);
        }

        /// <summary>
        /// Creates a Worksshift.
        /// </summary>
        /// <returns></returns>
        public WorkShift WorkShift()
        {
            return WorkShift(ActivityLayerList());
        }

        public WorkShift WorkShift(IList<ActivityLayer> actLayerList)
        {
            return new WorkShift("Test" + longString60, actLayerList, _oldShiftCat3);
        }

        /// <summary>
        /// Gets the act layer list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-30
        /// </remarks>
        public IList<ActivityLayer> ActivityLayerList()
        {
            IList<ActivityLayer> actLayerList = new List<ActivityLayer>();
            ActivityLayer oldActLayer = new ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(8, 9, 0), new TimeSpan(16, 17, 0)), _oldAct1);
            actLayerList.Add(oldActLayer);

            oldActLayer = new ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0)), _oldAct2);
            actLayerList.Add(oldActLayer);

            oldActLayer = new ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(15, 0, 0), new TimeSpan(15, 30, 0)), _oldAct3);
            actLayerList.Add(oldActLayer);

            return actLayerList;
        }

        /// <summary>
        /// Creates a FillUpShift.
        /// </summary>
        /// <returns></returns>
        public FillupShift FillUpShift()
        {
            IList<ActivityLayer> actLayerList = new List<ActivityLayer>();

            //add two layers with different activities, will create 3 projected layers
            ActivityLayer oldActLayer =
                new ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(8, 9, 0), new TimeSpan(16, 17, 0)), _oldAct1);
            actLayerList.Add(oldActLayer);

            oldActLayer = new ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0)), _oldAct2);
            actLayerList.Add(oldActLayer);


            return new FillupShift(ScheduleType, actLayerList);
        }

        public global::Domain.Absence Absence(string name, string shortName, bool useCountRules)
        {
            return
                new global::Domain.Absence(1, name, shortName, true, Color.DodgerBlue, true, useCountRules, false, true, true,
                                           null);
        }

        private void Setup()
        {
            _oldAct1 = new global::Domain.Activity(1, "qwert", Color.AliceBlue, false, false, false, false, true, false, false, false);
            _oldAct2 = new global::Domain.Activity(2, "oldLunch", Color.AliceBlue, false, false, false, false, true, false, false, false);
            _oldAct3 = new global::Domain.Activity(3, "sjukDelAvDag", Color.AliceBlue, false, false, false, false, true, false, false, false);

            _oldAbs1 =
                new global::Domain.Absence(1, "Semester", "SE", true, Color.DodgerBlue, true, true, false, true, true, null);

            _oldAbs2 =
                new global::Domain.Absence(2, "Sjuk", "SJ", true, Color.DarkViolet, true, true, false, false, true, _oldAct3);

            _absenceActivityList.Add(_oldAbs2,_oldAct3);

            _newAbs1 = AbsenceFactory.CreateAbsence("Semester", "SE", Color.DodgerBlue);
            _newAbs2 = AbsenceFactory.CreateAbsence("Sjuk", "SJ", Color.DarkViolet);

            _newAct1 = ActivityFactory.CreateActivity("xyz", Color.AliceBlue);
            _newAct2 = ActivityFactory.CreateActivity("newLunch", Color.AliceBlue);

            _oldAgent =
                new Agent(-1, "Kalle" + longString60, "Kula" + longString60, "Kalle@Kula.nu" + longString60, "", null,
                          null,
                          null, "Test note");

            _schedType = new SchedType(-1, "SchedType" + longString60, false, false, false);

            _oldShiftCat1 =
                new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true, 0);

            _oldShiftCat2 =
                new global::Domain.ShiftCategory(144, "Evening", "EV", Color.BlueViolet, true, true, 0);

            _oldShiftCat3 =
                new global::Domain.ShiftCategory(288, "Test" + longString60, "T" + longString60, Color.AliceBlue, true,
                                                 true, 0);

            _newShiftCat1 = new ShiftCategory("Morgon");
            _newShiftCat2 = new ShiftCategory("Kväll");
            _newShiftCat3 = new ShiftCategory(("Test" + longString60).Substring(0, 50));

            _oldScenario1 = new global::Domain.Scenario(3, "Low", true, true);
            _oldScenario2 = new global::Domain.Scenario(4, "High", true, true);

            _oldScenario3 =
                new global::Domain.Scenario(5, "Default" + longString60, false, false);

            _newScenario1 = new Scenario("Default1");

            _newScenario2 = new Scenario("Default2");

            _newScenario3 = new Scenario("Default3");

        }

        private class DatasourceChangeInfo : IChangeInfo
        {
            #region IChangeInfo Members

            User IChangeInfo.ChangedBy
            {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            DateTime IChangeInfo.ChangedDate
            {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            #endregion
        }
    }
}