using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
	[TestFixture]
	public class RequestDetailsPresenterTest
	{
		private MockRepository _mocks;
		private IRequestDetailsView _view;
		private IPersonRequestViewModel _model;
		private RequestDetailsPresenter _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.DynamicMock<IRequestDetailsView>();
			_model = _mocks.StrictMock<IPersonRequestViewModel>();
			_target = new RequestDetailsPresenter(_view, _model);
		}

		[Test]
		public void ShouldInitialize()
		{
			var target = new RequestDetailsPresenter(MockRepository.GenerateMock<IRequestDetailsView>(),
			                                         MockRepository.GenerateMock<IPersonRequestViewModel>());

			target.Initialize();
		}

		[Test]
		public void ShouldValidateShiftTradeRequest()
		{
			var shiftTradeRequest = CreateShiftTradeRequestObject();
			using (_mocks.Record())
			{
				Expect.Call(_model.PersonRequest).Return(shiftTradeRequest);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.IsShiftTradeRequest());
			}
		}

		[Test]
		public void ShouldValidateIfEditableOrNot()
		{
			using (_mocks.Record())
			{
				Expect.Call(_model.IsEditable).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.IsRequestEditable());
			}
		}

		private static IPersonRequest CreateShiftTradeRequestObject()
		{
			var tradingPerson = PersonFactory.CreatePerson("First", "Last");
			var tradeWithPerson = PersonFactory.CreatePerson("First1", "Last1");
			var period = new List<DateOnly>
			             	{
			             		DateOnly.Today.AddDays(1),
			             		DateOnly.Today.AddDays(2),
			             		DateOnly.Today.AddDays(3)
			             	};
			IList<IShiftTradeSwapDetail> shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>();
			foreach (DateOnly dateOnly in period)
			{
				shiftTradeSwapDetails.Add(new ShiftTradeSwapDetail(tradingPerson, tradeWithPerson, dateOnly, dateOnly));
			}
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetails);
			var request = new PersonRequest(tradingPerson, shiftTradeRequest);
			request.Pending();
			return request;
		}
	}
}
