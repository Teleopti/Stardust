var timepicker = $.fn.timepicker.Constructor.prototype;

timepicker.setTime = function (time) {
	var arr,
		timeArray;

	if (this.showMeridian) {
		arr = /^(.*?)([^\d:]*)$/.exec(time.trim());
		timeArray = arr[1].trim().split(':');
		this.meridian = arr[2].trim();
	} else {
		timeArray = time.split(':');
	}

	this.hour = parseInt(timeArray[0], 10);
	this.minute = parseInt(timeArray[1], 10);
	this.second = parseInt(timeArray[2], 10);

	if (isNaN(this.hour)) {
		this.hour = 0;
	}
	if (isNaN(this.minute)) {
		this.minute = 0;
	}

	if (this.showMeridian) {
		if (this.hour > 12) {
			this.hour = 12;
		} else if (this.hour < 1) {
			this.hour = 12;
		}

		if (!this.meridianMatchesAgentDateCulture()) {
			if (this.meridian === 'am' || this.meridian === 'a' || this.meridian === 'A') {
				this.meridian = 'AM';
			} else if (this.meridian === 'pm' || this.meridian === 'p' || this.meridian === 'P') {
				this.meridian = 'PM';
			}

			if (this.meridian !== 'AM' && this.meridian !== 'PM') {
				this.meridian = 'AM';
			}
		}
	} else {
		if (this.hour >= 24) {
			this.hour = 23;
		} else if (this.hour < 0) {
			this.hour = 0;
		}
	}

	if (this.minute < 0) {
		this.minute = 0;
	} else if (this.minute >= 60) {
		this.minute = 59;
	}

	if (this.showSeconds) {
		if (isNaN(this.second)) {
			this.second = 0;
		} else if (this.second < 0) {
			this.second = 0;
		} else if (this.second >= 60) {
			this.second = 59;
		}
	}

	this.update();
};

timepicker.meridianMatchesAgentDateCulture = function () {
	return (this.meridian === Teleopti.MyTimeWeb.Common.Meridiem.AM ||
		this.meridian === Teleopti.MyTimeWeb.Common.Meridiem.PM);
};

timepicker.toggleMeridian = function () {
	this.meridian = this.meridian === Teleopti.MyTimeWeb.Common.Meridiem.AM
		? Teleopti.MyTimeWeb.Common.Meridiem.PM
		: Teleopti.MyTimeWeb.Common.Meridiem.AM;
	this.update();
};

timepicker.setDefaultTime = function (defaultTime) {
	if (!this.$element.val()) {
		if (defaultTime === 'current') {
			var dTime = new Date(),
				hours = dTime.getHours(),
				minutes = Math.floor(dTime.getMinutes() / this.minuteStep) * this.minuteStep,
				seconds = Math.floor(dTime.getSeconds() / this.secondStep) * this.secondStep,
				meridian = Teleopti.MyTimeWeb.Common.Meridiem.AM;
			if (this.showMeridian) {
				if (hours === 0) {
					hours = 12;
				} else if (hours >= 12) {
					if (hours > 12) {
						hours = hours - 12;
					}
					meridian = Teleopti.MyTimeWeb.Common.Meridiem.PM;
				} else {
					meridian = Teleopti.MyTimeWeb.Common.Meridiem.AM;
				}
			}

			this.hour = hours;
			this.minute = minutes;
			this.second = seconds;
			this.meridian = meridian;

			this.update();

		} else if (defaultTime === false) {
			this.hour = 0;
			this.minute = 0;
			this.second = 0;
			this.meridian = Teleopti.MyTimeWeb.Common.Meridiem.AM;
		} else {
			this.setTime(defaultTime);
		}
	} else {
		this.updateFromElementVal();
	}
};
