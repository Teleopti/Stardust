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

	test('should set now correctly on IE', function() {
		var dateStr = '2018-03-04';
		Date.prototype.getTeleoptiTime = function() {
			return dateStr;
		};

		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return dateStr;
		};

		var target = Teleopti.MyTimeWeb.Asm;
		target._replaceAjax({
			Ajax: function(option) {
				option.success({ UserTimeZoneMinuteOffset: 0 });
				return {
					done: function() {}
				};
			}
		});

		var enableIntervalUpdate = false;
		target.ShowAsm(null, enableIntervalUpdate);

		equal(target.Vm().now().format('YYYY-MM-DDTHH:mm:ss'), '2018-03-04T00:00:00');
	});

	ignore('should update current and next activity label', function() {
		var dateStr = '2018-03-04';
		Date.prototype.getTeleoptiTime = function() {
			return dateStr;
		};

		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return dateStr;
		};

		var fakeData = {
			"Layers": [
				{
					"Payload": "Phone",
					"StartMinutesSinceAsmZero": 1440+115,
					"LengthInMinutes": 16,
					"Color": "#80FF80",
					"StartTimeText": "01: 45",
					"EndTimeText": "02: 01"
				},
				{
					"Payload": "Email",
					"StartMinutesSinceAsmZero": 1440+121,
					"LengthInMinutes": 15,
					"Color": "#80FF80",
					"StartTimeText": "02: 01",
					"EndTimeText": "02: 15"
				},
				{
					"Payload": "Chat",
					"StartMinutesSinceAsmZero": 1440+135,
					"LengthInMinutes": 15,
					"Color": "#80FF80",
					"StartTimeText": "02: 15",
					"EndTimeText": "02: 30"
				}
			],
			"Hours": ["0","1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23"
			],
			"UnreadMessageCount": 147,
			"UserTimeZoneMinuteOffset": 120
		};

		var target = Teleopti.MyTimeWeb.Asm;
		target._replaceAjax({
			Ajax: function(option) {
				option.success(fakeData);
				return {
					done: function() {}
				};
			}
		});

		var enableIntervalUpdate = false;
		target.ShowAsm(null, enableIntervalUpdate);

		equal(target.Vm().now().format('YYYY-MM-DDTHH:mm:ss'), '2018-03-04T02:00:00');
		equal(target.Vm().layers()[0].visible(), true);
		equal(target.Vm().currentLayerString(), '01: 45-02: 01 Phone');
		equal(target.Vm().nextLayerString(), '02: 01-02: 15 Email');

		stop();
		setTimeout(function() {
			start();
			target.Vm().now(moment('2018-03-04T02:00:01'));

			equal(target.Vm().now().format('YYYY-MM-DDTHH:mm:ss'), '2018-03-04T02:00:01');
			equal(target.Vm().layers()[0].visible(), false);
			equal(target.Vm().currentLayerString(), '02: 01-02: 15 Email');
			equal(target.Vm().nextLayerString(), '02: 15-02: 30 Chat');
		}, 1000);
	});

	function ignore() {}
});
