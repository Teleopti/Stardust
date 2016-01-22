using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class ShiftCreatorServiceTest
    {
        private MockRepository mocks;
        private ShiftCreatorService target;
        private IWorkShiftTemplateGenerator templateGen;
        private ICreateWorkShiftsFromTemplate buildFromOneTemplate;


        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            templateGen = mocks.StrictMock<IWorkShiftTemplateGenerator>();
            buildFromOneTemplate = mocks.StrictMock<ICreateWorkShiftsFromTemplate>();
            target = new ShiftCreatorService(buildFromOneTemplate);
        }

        [Test]
        public void VerifyGenerate()
        {
            IWorkShiftRuleSet ruleSet = mocks.StrictMock<IWorkShiftRuleSet>();
            ReadOnlyCollection<IWorkShiftExtender> ext = new List<IWorkShiftExtender>().AsReadOnly();
            ReadOnlyCollection<IWorkShiftLimiter> lim = new List<IWorkShiftLimiter>().AsReadOnly();
            IList<IWorkShift> templateGenReturn = new List<IWorkShift>();
            templateGenReturn.Add(WorkShiftFactory.Create(new TimeSpan(8), new TimeSpan(17)));
            templateGenReturn.Add(WorkShiftFactory.Create(new TimeSpan(9), new TimeSpan(18)));

            WorkShift ws1 = new WorkShift(new ShiftCategory("sdf"));
            WorkShift ws2 = new WorkShift(new ShiftCategory("sdf2"));
            IList<IWorkShift> templateGen1Return = new List<IWorkShift>();
            templateGen1Return.Add(ws1);
            IList<IWorkShift> templateGen2Return = new List<IWorkShift>();
            templateGen2Return.Add(ws2);
			var callback = new WorkShiftAddStopperCallback();
            using(mocks.Record())
            {
                Expect.Call(ruleSet.TemplateGenerator)
                    .Return(templateGen).Repeat.AtLeastOnce();
                Expect.Call(ruleSet.ExtenderCollection)
                    .Return(ext).Repeat.AtLeastOnce();
                Expect.Call(ruleSet.LimiterCollection)
                    .Return(lim).Repeat.AtLeastOnce();
                Expect.Call(templateGen.Generate())
                    .Return(templateGenReturn);
				Expect.Call(buildFromOneTemplate.Generate(templateGenReturn[0], ext, lim, null))
                    .Return(templateGen1Return).IgnoreArguments();
				Expect.Call(buildFromOneTemplate.Generate(templateGenReturn[1], ext, lim, null))
                    .Return(templateGen2Return).IgnoreArguments();
                Expect.Call(ruleSet.Description).Return(new Description());
            }
            using(mocks.Playback())
            {     
                IList<IWorkShift> ret = target.Generate(ruleSet, callback);
                Assert.AreEqual(2, ret.Count);
                Assert.IsTrue(ret.Contains(ws1));
                Assert.IsTrue(ret.Contains(ws2));
            }
        }
    }
}