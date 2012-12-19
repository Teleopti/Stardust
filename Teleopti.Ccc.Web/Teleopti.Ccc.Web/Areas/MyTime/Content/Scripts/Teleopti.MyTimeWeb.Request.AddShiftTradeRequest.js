/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />
/// <reference path="~/Content/Scripts/date.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel() {
		var self = this;
		var layerCanvasPixelWidth = 700;

		self.selectedDate = ko.observable(new Date('2013-11-29'));
		self.hasWorkflowControlSet = ko.observable(false);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.myScheduleLayers = ko.observableArray();

		self._createMyScheduleLayers = function (layers) {
			var map = ko.utils.arrayMap(layers, function (layer) {
				return new shiftTradeLayerViewModel(layer, self);
			});
			self.myScheduleLayers.push.apply(self.myScheduleLayers, map);
		};

		self.pixelPerMinute = ko.computed(function () {
			return layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});

		self.loadViewModel = function (date) {
			//console.log(date);
			ajax.Ajax({
				url: "Requests/ShiftTradeRequest",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				data: { selectedDate: date.toJSON() },
				success: function (data, textStatus, jqXHR) {
					self.hasWorkflowControlSet(!data.HasWorkflowControlSet);
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMyScheduleLayers(data.MySchedulelayers);
					//console.log(data);
				},
				error: function (err) {
					//console.log('Something went wrong here...');
					alert("error!");
					//console.log(err);
				}
			});
		};

		self.selectedDate.subscribe(function () {
			self.loadViewModel(self.selectedDate());
		});
	}

	function shiftTradeLayerViewModel(layer, parentViewModel) {
		var self = this;

		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.startTime = layer.StartTimeText;
		self.endTime = layer.EndTimeText;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.leftPx = ko.computed(function () {
			return layer.ElapsedMinutesSinceShiftStart * parentViewModel.pixelPerMinute() + 'px';
		});
		self.paddingLeft = ko.computed(function () {
			return self.lengthInMinutes * parentViewModel.pixelPerMinute() + 'px';
		});
		self.title = ko.computed(function () {
			return self.startTime + '-' + self.endTime + ' ' + self.payload;
		});

	}

	function _init() {
		//setShiftTradeRequestDate('2030-01-01'); //Just temporary
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
		//_activateDateButtons();		
		bindClickToOpenShiftTrade();
	}

	function bindClickToOpenShiftTrade() {
		$('#Request-add-shift-trade-button')
			.click(function () {
				$('#Request-add-shift-trade')
					.show();

				vm.loadViewModel($('#Request-add-selected-date').val());
				_initDatePicker();
			})
			;
	}

	function setShiftTradeRequestDate(date) {
		$('#Request-add-selected-date')
			.val(date)
			;
	}

	function _initDatePicker() {
		$('#Request-add-shift-trade-datepicker').datepicker({
			showAnim: "slideDown",
			onSelect: function (dateText, inst) {
				//alert(dateText);
				//alert('selectedDate = ' + inst.selectedYear + '-' + inst.selectedMonth + '-' + inst.selectedDay);
				//var fixedDate = _datePickerPartsToFixedDate(inst.selectedYear, inst.selectedMonth, inst.selectedDay);
			}
		});
	}

	return {
		Init: function () {
			_init();
		},
		SetShiftTradeRequestDate: function (date) {
			setShiftTradeRequestDate(date);
		}
	};

})(jQuery);

ko.bindingHandlers.datepicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		//initialize datepicker with some optional options
		var options = allBindingsAccessor().datepickerOptions || {};
		$(element).datepicker(options);

		//handle the field changing
		ko.utils.registerEventHandler(element, "change", function () {
			var observable = valueAccessor();
			observable($(element).datepicker("getDate"));
		});

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$(element).datepicker("destroy");
		});

	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor()),
            current = $(element).datepicker("getDate");
		//alert(current);
		if (value - current !== 0) {
			$(element).datepicker("setDate", value);
		}
	}
};