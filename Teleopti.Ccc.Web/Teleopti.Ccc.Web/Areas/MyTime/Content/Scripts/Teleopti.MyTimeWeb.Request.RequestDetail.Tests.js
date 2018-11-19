$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Request.RequestDetail');

	test('should pass FullDay to backend for absence request', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/PersonalAccountPermission') {
					options.success(true);
				}
			}
		};
		$('body').append('<div id="Request-add-data-binding-area"></div>');
		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function(cb) {
				cb &&
					cb({
						WeekStart: 1,
						DateFormatForMoment: 'DD/MM/YYYY',
						BaseUtcOffsetInMinutes: 60,
						DaylightSavingTimeAdjustment: {
							StartDateTime: '/Date(1521939600000)/',
							EndDateTime: '/Date(1540692000000)/',
							AdjustmentOffsetInMinutes: 60,
							LocalDSTStartTimeInMinutes: 0,
							EnteringDST: false
						}
					});
			}
		};

		Teleopti.MyTimeWeb.Common.Init({ baseUrl: '' }, ajax);
		
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM',
			DateTimeDefaultValues: {
				defaultStartTime: '08:00',
				defaultEndTime: '17:00',
				defaultFulldayStartTime: '00:00',
				defaultFulldayEndTime: '23:59',
				todayYear: '2018',
				todayMonth: '10',
				todayDay: '23'
			}
		});
		Teleopti.MyTimeWeb.Request.RequestDetail.PrepareForViewModel(Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);
		Teleopti.MyTimeWeb.Request.RequestDetail.Init(ajax);

		Teleopti.MyTimeWeb.Request.RequestDetail.AddAbsenceRequestClick();

		var requestViewModel = Teleopti.MyTimeWeb.Request.RequestDetail.ParentViewModel().requestViewModel();
		requestViewModel.Subject('test subject');
		requestViewModel.AddRequest();
		var formData = Teleopti.MyTimeWeb.Request.RequestDetail.GetFormData(requestViewModel);

		equal(formData.FullDay, true);

		requestViewModel.IsFullDay(false);
		var newFormData = Teleopti.MyTimeWeb.Request.RequestDetail.GetFormData(requestViewModel);

		equal(newFormData.FullDay, false);
	});
});
