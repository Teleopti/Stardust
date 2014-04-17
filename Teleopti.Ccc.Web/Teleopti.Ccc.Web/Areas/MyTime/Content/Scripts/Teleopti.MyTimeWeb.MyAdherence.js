Teleopti.MyTimeWeb.MyAdherence = (function () {
	var vm;

	function MyAdherenceViewModel(date) {
		var self = this;

		self.selectedDateInternal = ko.observable(date);
		self.datePickerFormat = ko.observable('YYYYMMDD');
		//var format = $('#my-report-datepicker-format').val().toUpperCase();
		//self.datePickerFormat(format);
		self.goToAnotherDay = function (toDate) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/Adherence" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(toDate.format('YYYY-MM-DD')));
		};
		self.selectedDate = ko.computed({
			read: function () {
				return self.selectedDateInternal();
			},
			write: function (value) {
				if (value.format('YYYYMMDD') == date.format('YYYYMMDD')) return;
				self.goToAnotherDay(value);
			}
		});
		self.nextDay = function () {
			self.goToAnotherDay(self.selectedDate().clone().add('days', 1));
		};
		self.previousDay = function() {
			self.goToAnotherDay(self.selectedDate().clone().add('days', -1));
		};
		self.dateFormat = function() {
			return self.datePickerFormat;
		};
	}

	function setWeekStart() {
		$.ajax({
			url: 'UserInfo/Culture',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
				ko.applyBindings(vm, $('div.navbar')[1]);
			}
		});
	};

	function bindData() {
		vm = new MyAdherenceViewModel(getDate());
		var elementToBind = $('.myadherence')[0];
		ko.applyBindings(vm, elementToBind);
	}
	
	function getDate() {
		var date = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
		if (date != '') {
			return moment(date, "YYYYMMDD");
		} else {
			return moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day');
		}
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('MyReport/Adherence',
									Teleopti.MyTimeWeb.MyAdherence.MyAdherencePartialInit);
		},

		MyAdherencePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			if (!$('.myadherence').length) {
				return;
			}

			bindData();
			setWeekStart();

			readyForInteractionCallback();
			completelyLoadedCallback();
		}
	};
})(jQuery);