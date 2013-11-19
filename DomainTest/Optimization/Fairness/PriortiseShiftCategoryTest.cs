using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.Fairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.Fairness
{
    //[TestFixture]
    //public class PriortiseShiftCategoryTest
    //{
    //    private IPriortiseShiftCategory _target;
    //    private ShiftCategory _sc4;
    //    private ShiftCategory _sc1;
    //    private ShiftCategory _sc2;
    //    private ShiftCategory _sc3;

    //    [SetUp]
    //    public void Setup()
    //    {
    //        _target = new PriortiseShiftCategory();
    //        _sc1 = ShiftCategoryFactory.CreateShiftCategory("class");
    //        _sc2 = ShiftCategoryFactory.CreateShiftCategory("dump");
    //        _sc3 = ShiftCategoryFactory.CreateShiftCategory("sky");
    //        _sc4 = ShiftCategoryFactory.CreateShiftCategory("argos");

    //        IList<IShiftCategory> shiftCategories = new List<IShiftCategory> {_sc1, _sc2, _sc3, _sc4};
    //        _target.GetPriortiseShiftCategories(shiftCategories);
    //    }

    //    [Test]
    //    public void TestHighPriority()
    //    {
    //        Assert.AreEqual(_target.HigestPriority,4 );
    //    }

    //    [Test]
    //    public void TestLowPriority()
    //    {
    //        Assert.AreEqual(_target.LowestPriority, 1);
    //    }

    //    [Test]
    //    public void TestShiftCategoryOnPriority()
    //    {
    //        Assert.AreEqual(_target.ShiftCategoryOnPriority(1), _sc1);
    //        Assert.AreEqual(_target.ShiftCategoryOnPriority(2), _sc2);
    //        Assert.AreEqual(_target.ShiftCategoryOnPriority(3), _sc3);
    //        Assert.AreEqual(_target.ShiftCategoryOnPriority(4), _sc4);
    //    }

    //    [Test]
    //    public void TestPriorityOnPriority()
    //    {
    //        Assert.AreEqual(_target.PriorityOfShiftCategory(_sc1),1);
    //        Assert.AreEqual(_target.PriorityOfShiftCategory(_sc2),2);
    //        Assert.AreEqual(_target.PriorityOfShiftCategory(_sc3),3);
    //        Assert.AreEqual(_target.PriorityOfShiftCategory(_sc4),4);
    //    }
    //}
}
