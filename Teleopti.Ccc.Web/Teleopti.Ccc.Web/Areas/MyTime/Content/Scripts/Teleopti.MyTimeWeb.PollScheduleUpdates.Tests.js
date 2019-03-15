$(document).ready(function() {
	var target = Teleopti.MyTimeWeb.PollScheduleUpdates;
	var common = Teleopti.MyTimeWeb.Common;
	var notifier = Teleopti.MyTimeWeb.Notifier;
	var alertActivity = Teleopti.MyTimeWeb.AlertActivity;
	var intervalTimeout = 5 * 60 * 1000; // hardcode to 5 min
	var currentText;
	var notifyText = 'Your schedule has changed!';

	var realAjax = Teleopti.MyTimeWeb.Ajax;
	var realGetTeleoptiTime = Date.prototype.getTeleoptiTime;
	var realNotify = notifier.Notify;
	var realGetNotificationDisplayTime = alertActivity.GetNotificationDisplayTime;
	var hasUpdates = true;
	module('Teleopti.MyTimeWeb.PollScheduleUpdates', {
		setup: function() {
			fakeAjax();
			var options = {
				UserTimezoneOffsetMinute: 0,
				HasDayLightSaving: 'False',
				DayLightSavingStart: null,
				DayLightSavingEnd: null,
				DayLightSavingAdjustmentInMinute: null
			};
			Date.prototype.getTeleoptiTime = common.SetupTeleoptiTime(options);
			notifier.Notify = function(setting, noticeText) {
				currentText = noticeText;
			};
			alertActivity.GetNotificationDisplayTime = function(callback) {
				callback();
			};
		},
		teardown: function() {
			Teleopti.MyTimeWeb.Ajax = realAjax;
			Date.prototype.getTeleoptiTime = realGetTeleoptiTime;
			notifier.Notify = realNotify;
			alertActivity.GetNotificationDisplayTime = realGetNotificationDisplayTime;
			target.Destroy();
		}
	});

	test('Should show notice if schedule changed within correct period', function() {
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(currentText, target.GetNotifyText());
		target.Destroy();
	});

	test('Should not show notice if no schedule changed within correct period', function() {
		hasUpdates = false;
		currentText = null;
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(currentText, null);
		hasUpdates = true;
		target.Destroy();
	});

	test('Should call listener callback if has schedule change within  period', function() {
		var called = false;
		target.AddListener('Schedule/Week', function() {
			called = true;
		});
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(called, true);
		target.Destroy();
	});

	test('Should all listeners be invoked when time is up', function() {
		var executedListeners = [];
		target.AddListener('Schedule/Week', function() {
			executedListeners.push('Schedule/Week');
		});
		target.AddListener('Schedule/ASM', function() {
			executedListeners.push('Schedule/ASM');
		});
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(executedListeners[0], 'Schedule/Week');
		equal(executedListeners[1], 'Schedule/ASM');
		target.Destroy();
	});

	test('Should only registe one time for same name', function() {
		var register = '';
		target.AddListener('Schedule/Week', function() {
			register = 'first';
		});
		target.AddListener('Schedule/Week', function() {
			register = 'second';
		});
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(register, 'first');
		target.Destroy();
	});

	function fakeAjax() {
		Teleopti.MyTimeWeb.Ajax = function() {
			return {
				Ajax: function(options) {
					switch (options.url) {
						case 'Asm/StartPolling':
							options.success('mailboxId');
							break;
						case 'Asm/CheckIfScheduleHasUpdates':
							options.success({ HasUpdates: hasUpdates });
							break;
						case 'Asm/NotificationsTimeToStaySetting':
							options.success(1000);
							break;
						case 'UserData/FetchUserData':
							options.success({
								BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
								DataSourceName: 'Teleopti WFM',
								Url: 'http://localhost:52858/TeleoptiWFM/Web/',
								AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
							});
							break;
					}
				}
			};
		};
		return Teleopti.MyTimeWeb.Ajax;
	}
});
