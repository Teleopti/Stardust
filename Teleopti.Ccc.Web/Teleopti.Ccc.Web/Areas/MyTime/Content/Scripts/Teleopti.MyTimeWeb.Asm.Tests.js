$(document).ready(function() {
	var oldAjax, tempFn1, tempFn2;

	module("Teleopti.MyTimeWeb.Asm", {
		setup: function() {
			tempFn1 = Date.prototype.getTeleoptiTime;
			Date.prototype.getTeleoptiTime = function() {
				return new Date("2018-03-05T02:30:00Z").getTime();
			};

			tempFn2 = Date.prototype.getTeleoptiTimeInUserTimezone;
			Date.prototype.getTeleoptiTimeInUserTimezone = function() {
				return "2018-03-04";
			};

			Teleopti.MyTimeWeb.Common.FakeToggles({
				MyTimeWeb_PollToCheckScheduleChanges_46595: false
			});
			oldAjax = Teleopti.MyTimeWeb.Ajax;
		},
		teardown: function() {
			Teleopti.MyTimeWeb.Ajax = oldAjax;

			Date.prototype.getTeleoptiTime = tempFn1;
			Date.prototype.getTeleoptiTimeInUserTimezone = tempFn2;
		}
	});

	test("Should fetch layer with yesterday under user timezone", function() {
		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return "2018-03-04";
		};

		var target = Teleopti.MyTimeWeb.Asm;
		var ajaxOption;
		target._replaceAjax({
			Ajax: function(option) {
				ajaxOption = option;
				option.success({ UserTimeZoneMinuteOffset: -450 });
				return {
					done: function() {}
				};
			}
		});

		var enableIntervalUpdate = false;
		target.ShowAsm(null, enableIntervalUpdate);

		equal(ajaxOption.data.asmZeroLocal, "2018-03-03");
	});
});
