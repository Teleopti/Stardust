using System;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class AbsenceRequestPresenterTest
    {
        private MockRepository _mocks;
        private IAbsenceRequestView _view;
        private AbsenceRequestPresenter _target;
        private ITeleoptiSchedulingService _teleoptiSchedulingService;
        private DateTimePeriodDto _dateTimePeriodDto;
        private PersonDto _personDto;
        private AbsenceDto _absenceDto;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<IAbsenceRequestView>();
            _teleoptiSchedulingService = _mocks.StrictMock<ITeleoptiSchedulingService>();
            _personDto = CreatePerson();
            _absenceDto = CreateAbsence();

            DateTime startDateTime = DateTime.Now;
            DateTime endDateTime = startDateTime.AddDays(1);
            _dateTimePeriodDto = CreateDateTimePeriodDto(startDateTime, endDateTime);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "asdf"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortalCode.Requests.IAbsenceRequestView.ShowDeleteErrorMessage(System.String)"), Test]
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
				_target = new AbsenceRequestPresenter(_teleoptiSchedulingService,_view,personRequest);
				_target.InitializeView();
				_target.Delete().Should().Be.False();
			}
		}

        [Test]
        public void CanSetAllDayState()
        {
            using (_mocks.Record())
            {
				Expect.Call(_view.AbsenceType).Return(_absenceDto);

                _view.TimePickersEnabled = false;
                Expect.Call(_view.TimePickerTimePeriod).Return(new TimePeriod(TimeSpan.Zero, TimeSpan.Zero)).Repeat.Any();
                Expect.Call(_view.TimePickersEnabled).Return(false).Repeat.Any();
                Expect.Call(_view.SelectedStartDateTime).Return(_dateTimePeriodDto.LocalStartDateTime).Repeat.Any();
                Expect.Call(_view.SelectedEndDateTime).Return(_dateTimePeriodDto.LocalEndDateTime).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _personDto, _view, _dateTimePeriodDto);
             	_target.ChangeAllDayState(CheckState.Checked);

                Assert.IsFalse(_view.TimePickersEnabled);
                Assert.AreEqual(new TimePeriod(TimeSpan.Zero, TimeSpan.Zero), _view.TimePickerTimePeriod);
            }
        }

        [Test]
        public void CanUncheckAllDayState()
        {
            using (_mocks.Record())
            {
                Expect.Call(_view.AbsenceType).Return(_absenceDto);

                _view.TimePickersEnabled = true;
                Expect.Call(_view.TimePickerTimePeriod).Return(new TimePeriod(_dateTimePeriodDto.LocalStartDateTime.TimeOfDay, _dateTimePeriodDto.LocalEndDateTime.TimeOfDay)).Repeat.Any();
                Expect.Call(_view.TimePickersEnabled).Return(true).Repeat.Any();
                Expect.Call(_view.SelectedStartDateTime).Return(_dateTimePeriodDto.LocalStartDateTime).Repeat.Any();
                Expect.Call(_view.SelectedEndDateTime).Return(_dateTimePeriodDto.LocalEndDateTime).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _personDto, _view, _dateTimePeriodDto);
                _target.ChangeAllDayState(CheckState.Unchecked);
                Assert.True(_view.TimePickersEnabled);
                Assert.AreEqual(new TimePeriod(_dateTimePeriodDto.LocalStartDateTime.TimeOfDay, _dateTimePeriodDto.LocalEndDateTime.TimeOfDay), _view.TimePickerTimePeriod);
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
            
            using(_mocks.Playback())
            {
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _view, personRequest);
                _target.InitializeView();
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
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _view, personRequest);
                _target.InitializeView();
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
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _view, personRequest);
                _target.InitializeView();
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
                _target = new AbsenceRequestPresenter(_teleoptiSchedulingService, _view, personRequest);
                _target.InitializeView();
            }
        }

        private static AbsenceDto CreateAbsence()
        {
            AbsenceDto absenceDto = new AbsenceDto();
            absenceDto.Id = Guid.NewGuid().ToString();
            absenceDto.Name = "Holiday";
            return absenceDto;
        }

        private static PersonDto CreatePerson()
        {
            PersonDto personDto = new PersonDto();
            personDto.Name = "Fidel Castro";
            personDto.Email = "fidel@cuba.cu";
            return personDto;
        }

        private static DateTimePeriodDto CreateDateTimePeriodDto(DateTime localStartDateTime, DateTime localEndDateTime)
        {
            DateTimePeriodDto period = new DateTimePeriodDto();
            period.LocalStartDateTime = localStartDateTime;
            period.LocalEndDateTime = localEndDateTime;
            period.UtcStartTimeSpecified = false;
            period.UtcEndTimeSpecified = false;
            return period;
        }
    }
}
