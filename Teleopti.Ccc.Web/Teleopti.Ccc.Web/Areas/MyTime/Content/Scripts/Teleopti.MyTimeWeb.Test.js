Teleopti.MyTimeWeb.Test = (function($) {
	var _settings = {};
	var _messages = [];
	var _displayEnabled = false;

	function _testMessage(message) {
		_messages.push(message);
		if (_displayEnabled) _displayMessage(message);
	}

	function _getTestMessages() {
		var messages = '';
		_displayEnable();
		for (var i = 0; i < _messages.length; i++) {
			messages = messages + _messages[i];
		}
		return messages;
	}

	function _displayEnable() {
		if (!_displayEnabled) {
			_displayEnabled = true;
			for (var i = 0; i < _messages.length; i++) {
				_displayMessage(_messages[i]);
			}
		}
	}

	function _displayMessage(message) {
		$('#page')
			.append(message)
			.append($('<br>'));
	}

	function _expireMyCookie(message) {
		$.ajax({
			url: _settings.startBaseUrl + 'Test/ExpireMyCookie',
			global: false,
			cache: false,
			async: false,
			success: function() {
				$.ajax({
					url: _settings.startBaseUrl + 'Menu/Applications',
					global: false,
					cache: false,
					async: false,
					success: function() {},
					error: function(r) {
						if (r.status == 401)
							//401 = not logged in
							_testMessage(message);
					}
				});
			}
		});
	}

	return {
		Init: function(settings) {
			_settings = settings;
		},
		TestMessage: function(message) {
			_testMessage(message);
		},
		GetTestMessages: function() {
			return _getTestMessages();
		},
		ExpireMyCookie: function(message) {
			_expireMyCookie(message);
		}
	};
})(jQuery);

//Globals for tests
var requestsMessagesUserTexts = {
	MISSING_SUBJECT: 'Missing subject',
	MISSING_OVERTIME_TYPE: 'Missing overtime type',
	MISSING_STARTTIME: 'Missing start time',
	MISSING_DURATION: 'Missing duration',
	OVERTIME_REQUEST_DATE_IS_PAST: 'Can not add overtime request on past date',
	ENDTIME_MUST_BE_GREATER_THAN_STARTTIME: 'End time must be greater than start time'
};

Teleopti.MyTimeWeb.Common.Init(
	{
		defaultNavigation: '/',
		baseUrl: '/',
		startBaseUrl: '/'
	},
	{
		Ajax: function(option) {}
	}
);

Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';

Teleopti.MyTimeWeb.Common.SetUserTexts({
	XRequests: '@Resources.XRequests',
	SubjectColon: '@Resources.SubjectColon',
	LocationColon: '@Resources.LocationColon',
	DescriptionColon: '@Resources.DescriptionColon',
	ChanceOfGettingAbsenceRequestGranted: '@Resources.ChanceOfGettingAbsenceRequestGranted: ',
	SeatBookings: '@Resources.SeatBookings',
	YouHaveNotBeenAllocatedSeat: '@Resources.YouHaveNotBeenAllocatedSeat',
	Fair: '@Resources.Fair',
	Poor: '@Resources.Poor',
	Good: '@Resources.Good',
	High: '@Resources.High',
	Low: '@Resources.Low',
	ProbabilityToGetAbsenceColon: '@Resources.ProbabilityToGetAbsenceColon',
	ProbabilityToGetOvertimeColon: '@Resources.ProbabilityToGetOvertimeColon',
	HideStaffingInfo: '@Resources.HideStaffingInfo',
	ShowAbsenceProbability: '@Resources.ShowAbsenceProbability',
	ShowOvertimeProbability: '@Resources.ShowOvertimeProbability',
	StaffingInfo: '@Resources.StaffingInfo',
	NoDateIsSelected: '@Resources.NoDateIsSelected',
	FilterTeamSchedules: '@Resources.FilterTeamSchedules',
	StartTime: '@Resources.StartTime',
	EndTime: '@Resources.EndTime',
	ShowOnlyNightShifts: '@Resources.ShowOnlyNightShifts'
});
