using System;
using System.Web.Services.Protocols;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests
{
    [TestFixture]
    public class TextRequestPresenterTest
    {
        private MockRepository _mocks;
        private ITextRequestView _view;
        private TextRequestPresenter _target;
        private ITeleoptiSchedulingService _teleoptiSchedulingService;
        private PersonDto _personDto;

        [SetUp]
        public void Setup()
        {
			_mocks = new MockRepository();
			_view = _mocks.DynamicMock<ITextRequestView>();
            
            _teleoptiSchedulingService = _mocks.StrictMock<ITeleoptiSchedulingService>();
            _personDto = CreatePerson();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyExceptionOnSaveIfNotValid()
        {
            var requestDto = new RequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto };
            using (_mocks.Record())
            {
                _teleoptiSchedulingService.SavePersonRequest(personRequest);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
                _target.Save();
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyExceptionOnSaveIfNotValidDates()
        {
            var requestDto = new TextRequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto, Subject = "sub"};
            
            using (_mocks.Record())
            {
                Expect.Call(_view.Subject).Return(personRequest.Subject).Repeat.Any();
                _teleoptiSchedulingService.SavePersonRequest(personRequest);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
                _target.Save();
            }
        }

        [Test]
        public void VerifySave()
        {
            var requestDto = new TextRequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto, Subject = "sub" };

            using (_mocks.Record())
            {
                Expect.Call(_view.Subject).Return(personRequest.Subject).Repeat.Any();
                Expect.Call(_view.StartDateTime).Return(new DateTime(2008, 12, 27)).Repeat.Any();
                Expect.Call(_view.EndDateTime).Return(new DateTime(2008, 12, 28)).Repeat.Any();
                Expect.Call(_teleoptiSchedulingService.SavePersonRequest(personRequest)).Return(personRequest);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
                _target.Save();
            }
        }

        [Test]
        public void VerifyDelete()
        {
            var requestDto = new TextRequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto, Subject = "sub" };
            personRequest.Id = "12";
            using (_mocks.Record())
            {
                Expect.Call(()=>_teleoptiSchedulingService.DeletePersonRequest(personRequest));
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
				_target.Delete().Should().Be.True();
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "asdf"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortalCode.Requests.ITextRequestView.ShowDeleteErrorMessage(System.String)"), Test]
		public void ShouldExitGracefullyWhenExceptionOnDelete()
		{
			var requestDto = new TextRequestDto();
			requestDto.Period = new DateTimePeriodDto();

			var personRequest = new PersonRequestDto { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto, Subject = "sub" };
			personRequest.Id = "12";

			var exception = new SoapException("asdf", XmlQualifiedName.Empty);

			using (_mocks.Record())
			{
				Expect.Call(() => _view.ShowDeleteErrorMessage("asdf"));
				Expect.Call(() => _teleoptiSchedulingService.DeletePersonRequest(personRequest)).Throw(exception);
			}

			using (_mocks.Playback())
			{
				_target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
				_target.Initialize();
				_target.Delete().Should().Be.False();
			}
		}

        [Test]
        public void VerifyCanCreateModel()
        {
        	using (_mocks.Record())
        	{
        	}
			using (_mocks.Playback())
			{
				_target = new TextRequestPresenter(_view, _personDto, new DateTimePeriodDto(), _teleoptiSchedulingService);
				_target.Initialize();
				_target.Delete();
				_target.PersonRequest.Should().Not.Be.Null();
			}
        }

        [Test]
        public void VerifySetDenyTextIsCalled()
        {
            var requestDto = new RequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto() { DenyReason = "xxRequestDenyReasonSupervisor", Request = requestDto };
            using (_mocks.Record())
            {
                _view.DenyReason = LanguageResourceHelper.Translate("xxRequestDenyReasonSupervisor");
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
            }
        }

        [Test]
        public void VerifySetDenyTextIsSetToInvisible()
        {
            var requestDto = new RequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto() { DenyReason = "", Request = requestDto };
            using (_mocks.Record())
            {
                _view.DenyReason = LanguageResourceHelper.Translate("");
                _view.SetDenyReasonVisible(false);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
            }
        }

        [Test]
        public void VerifyCanSetFormReadOnlyIfApproved()
        {
            var requestDto = new RequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto() { Request = requestDto };
            personRequest.RequestStatus = RequestStatusDto.Approved;

            using (_mocks.Record())
            {
                _view.SetFormReadOnly(true);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
            }
        }

        [Test]
        public void VerifyCanSetFormReadOnlyIfDenied()
        {
            var requestDto = new RequestDto();
            requestDto.Period = new DateTimePeriodDto();

            var personRequest = new PersonRequestDto() { Request = requestDto };
            personRequest.RequestStatus = RequestStatusDto.Approved;

            using (_mocks.Record())
            {
                _view.SetFormReadOnly(true);
            }

            using (_mocks.Playback())
            {
                _target = new TextRequestPresenter(_view, personRequest, _teleoptiSchedulingService);
                _target.Initialize();
            }
        }

        private static PersonDto CreatePerson()
        {
            PersonDto personDto = new PersonDto();
            personDto.Name = "Tommy Tong";
            personDto.Email = "tommy.tong@kungfu.ch";
            return personDto;
        }
    }
}
