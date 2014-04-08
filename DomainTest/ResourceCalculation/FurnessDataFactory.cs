using System;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    public static class FurnessDataFactory
    {

        public static FurnessData CreateFurnessDataForTestSet1()
        {
            FurnessData ret = new FurnessData(3, 3);
            ret.ProductionDemand()[0] = 9;
            ret.ProductionDemand()[1] = 8;
            ret.ProductionDemand()[2] = 8;

            ret.ProducerResources()[0] = 6;
            ret.ProducerResources()[1] = 7;
            ret.ProducerResources()[2] = 8;

            ret.ProductivityMatrix()[0, 0] = 1.5;
            ret.ProductivityMatrix()[0, 1] = 1.5;
            ret.ProductivityMatrix()[0, 2] = 1.0;

            ret.ProductivityMatrix()[1, 0] = 2.0;
            ret.ProductivityMatrix()[1, 1] = 0.5;
            ret.ProductivityMatrix()[1, 2] = 0.0;

            ret.ProductivityMatrix()[2, 0] = 0.0;
            ret.ProductivityMatrix()[2, 1] = 1.0;
            ret.ProductivityMatrix()[2, 2] = 1.5;

            ret.ResourceMatrix()[0, 0] = 1;
            ret.ResourceMatrix()[0, 1] = 1;
            ret.ResourceMatrix()[0, 2] = 1;

            ret.ResourceMatrix()[1, 0] = 1;
            ret.ResourceMatrix()[1, 1] = 1;
            ret.ResourceMatrix()[1, 2] = 0;

            ret.ResourceMatrix()[2, 0] = 0;
            ret.ResourceMatrix()[2, 1] = 1;
            ret.ResourceMatrix()[2, 2] = 1;

            return ret;
        }

        public static FurnessData CreateFurnessDataForTestSet2()
        {
            FurnessData ret = new FurnessData(3, 2);
            ret.ProductionDemand()[0] = 500;
            ret.ProductionDemand()[1] = 1000;

            ret.ProducerResources()[0] = 60;
            ret.ProducerResources()[1] = 40;
            ret.ProducerResources()[2] = 100;

            ret.ProductivityMatrix()[0, 0] = 1;
            ret.ProductivityMatrix()[0, 1] = 1;

            ret.ProductivityMatrix()[1, 0] = 1;
            ret.ProductivityMatrix()[1, 1] = 0;

            ret.ProductivityMatrix()[2, 0] = 1;
            ret.ProductivityMatrix()[2, 1] = 1;

            ret.ResourceMatrix()[0, 0] = 1;
            ret.ResourceMatrix()[0, 1] = 1;

            ret.ResourceMatrix()[1, 0] = 1;
            ret.ResourceMatrix()[1, 1] = 0;

            ret.ResourceMatrix()[2, 0] = 1;
            ret.ResourceMatrix()[2, 1] = 1;

            return ret;
        }

        public static FurnessData CreateFurnessDataForTestSet3()
        {
            FurnessData ret = new FurnessData(3, 3);
            ret.ProductionDemand()[0] = 12;
            ret.ProductionDemand()[1] = 2.4;
            ret.ProductionDemand()[2] = 0;

            ret.ProducerResources()[0] = 12;
            ret.ProducerResources()[1] = 3;
            ret.ProducerResources()[2] = 0;

            ret.ProductivityMatrix()[0, 0] = 1;
            ret.ProductivityMatrix()[0, 1] = 0;
            ret.ProductivityMatrix()[0, 2] = 0;

            ret.ProductivityMatrix()[1, 0] = 1;
            ret.ProductivityMatrix()[1, 1] = 1;
            ret.ProductivityMatrix()[1, 2] = 0;

            ret.ProductivityMatrix()[2, 0] = 0;
            ret.ProductivityMatrix()[2, 1] = 0;
            ret.ProductivityMatrix()[2, 2] = 0;

            ret.ResourceMatrix()[0, 0] = 1;
            ret.ResourceMatrix()[0, 1] = 0;
            ret.ResourceMatrix()[0, 2] = 0;

            ret.ResourceMatrix()[1, 0] = 1;
            ret.ResourceMatrix()[1, 1] = 1;
            ret.ResourceMatrix()[1, 2] = 0;

            ret.ResourceMatrix()[2, 0] = 0;
            ret.ResourceMatrix()[2, 1] = 0;
            ret.ResourceMatrix()[2, 2] = 0;

            return ret;
        }

        public static FurnessData CreateFurnessDataForTestSetZeroDemand()
        {
            FurnessData ret = new FurnessData(3, 3);
            ret.ProductionDemand()[0] = 9;
            ret.ProductionDemand()[1] = 8;
            ret.ProductionDemand()[2] = 0;

            ret.ProducerResources()[0] = 6;
            ret.ProducerResources()[1] = 7;
            ret.ProducerResources()[2] = 8;

            ret.ProductivityMatrix()[0, 0] = 1.5;
            ret.ProductivityMatrix()[0, 1] = 1.5;
            ret.ProductivityMatrix()[0, 2] = 1.0;

            ret.ProductivityMatrix()[1, 0] = 2.0;
            ret.ProductivityMatrix()[1, 1] = 0.5;
            ret.ProductivityMatrix()[1, 2] = 0.0;

            ret.ProductivityMatrix()[2, 0] = 0.0;
            ret.ProductivityMatrix()[2, 1] = 0.0;
            ret.ProductivityMatrix()[2, 2] = 0.5;

            ret.ResourceMatrix()[0, 0] = 1;
            ret.ResourceMatrix()[0, 1] = 1;
            ret.ResourceMatrix()[0, 2] = 0;

            ret.ResourceMatrix()[1, 0] = 1;
            ret.ResourceMatrix()[1, 1] = 1;
            ret.ResourceMatrix()[1, 2] = 0;

            ret.ResourceMatrix()[2, 0] = 0;
            ret.ResourceMatrix()[2, 1] = 0;
            ret.ResourceMatrix()[2, 2] = 1;

            return ret;
        }

    }
}
