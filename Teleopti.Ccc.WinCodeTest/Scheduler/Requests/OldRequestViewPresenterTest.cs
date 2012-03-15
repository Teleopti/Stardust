using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class OldRequestViewPresenterTest
    {
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            
        }


    }

    public class OldRequestViewPresenter
    {
        private readonly IOldRequestView _oldRequestView;

        public OldRequestViewPresenter(IOldRequestView oldRequestView)
        {
            _oldRequestView = oldRequestView;
        }
    }

    public interface IOldRequestView
    {
    }
}