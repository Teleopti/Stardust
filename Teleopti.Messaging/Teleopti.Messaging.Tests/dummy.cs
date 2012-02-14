using NUnit.Framework;
using Rhino.Mocks;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class dummy
    {
        private MockRepository mocks;
        private ICalculatorService calc;

        [SetUp]
        public void sdfsdf()
        {
            mocks = new MockRepository();
            calc = mocks.CreateMock<ICalculatorService>();
        }

        [Test]
        public void sdfsdfsdf()
        {
            SomeClass obj = new SomeClass(calc); //inject in some way

            using(mocks.Record())
            {
                //this must happend
                Expect.Call(calc.Add(2, 4))
                    .Return(6);
            }
            using(mocks.Playback())
            {
                StringAssert.AreEqualIgnoringCase("6", obj.DoSomething(2,4));
            }
        }
    }

    internal class SomeClass
    {
        private readonly ICalculatorService _calc;

        public SomeClass(ICalculatorService calc)
        {
            _calc = calc;
        }


        public string DoSomething(int a, int b)
        {
            return _calc.Add(a, b).ToString();
        }
    }


    public interface ICalculatorService
    {
        int Add(int a, int b);
    }

}
