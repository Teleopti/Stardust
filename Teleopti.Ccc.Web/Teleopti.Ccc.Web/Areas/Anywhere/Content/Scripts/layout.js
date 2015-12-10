
define([
		'text!templates/layout.html',
		'text!templates/menu.html',
		'text!templates/error.html',
		'text!templates/notification.html',
		'crossroads',
		'hasher',
		'knockout',
		'moment',
		'momentDatepickerKo',
		'menu',
		'ajax',
		'errorview',
		'resources',
		'subscriptions.trackingmessages',
		'notifications',
		'shared/timezone-current',
		'signalrhubs'
],	function (
		layoutTemplate,
		menuTemplate,
		errorTemplate,
		notificationTemplate,
		crossroads,
		hasher,
		ko,
		moment,
		datepicker,
		menuViewModel,
		ajax,
		errorview,
		resources,
		trackingmessages,
		notificationsViewModel,
		timezoneCurrent,
		signalrhubs) {

	var defaultView = 'teamschedule';
	
	var menu = new menuViewModel(resources);
	var contentPlaceHolder;
	var defaultBu;
	var trackingPersonId;

	function _displayView(routeInfo) {

		errorview.remove();

		routeInfo.renderHtml = function (html) {
			contentPlaceHolder.html(html);
		};

		routeInfo.bindingElement = contentPlaceHolder[0];

		var module = 'views/' + routeInfo.view + '/view';
		require([module], function (view) {

			if (view == undefined) {
				errorview.display("View " + routeInfo.view + " could not be loaded");
				return;
			}

			view.ready = false;

			if (view && view.dispose)
				view.dispose(routeInfo);
			view.initialize(routeInfo);

			var promise = view.display(routeInfo);
			if (view.clearaction)
				view.clearaction(routeInfo);
			if (routeInfo.action)
				view[routeInfo.action](routeInfo);

			if (promise) {
				promise.done(function () {

					view.ready = true;
				});
			} else {
				view.ready = true;
			}

			_fixBootstrapDropdownForMobileDevices();
		});

		menu.CurrentBusinessUnitId(routeInfo.buid);
		menu.CurrentGroupId(routeInfo.id);
		menu.CurrentDate(routeInfo.date);
		menu.ActiveView(routeInfo.view);
		if (trackingPersonId)
			trackNotification(trackingPersonId);
	}
	
	function _setupRoutes() {
		var viewRegex = '[a-z]+';
		var actionRegex = '[a-z]+';
		var guidRegex = '[a-z0-9]{8}(?:-[a-z0-9]{4}){3}-[a-z0-9]{12}';
		var dateRegex = '\\d{8}';
		var timeRegex = '[-]*\\d*';

		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')/(' + guidRegex + ')$', "i"),
			function (view,buid, id, date, action, secondaryId) {
				_displayView({ view: view, buid: buid, id: id, date: date, action: action, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + actionRegex + ')/(' + guidRegex + ')$', "i"),
			function (view,buid, id, action, secondaryId) {
				_displayView({ view: view, buid: buid, id: id, action: action, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')$', "i"),
			function (view,buid, personid, date, action) {
				_displayView({ view: view, buid: buid, personid: personid, date: date, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')$', "i"),
			function (view,buid, groupid, personid, date, action) {
				_displayView({ view: view,buid: buid, groupid: groupid, personid: personid, date: date, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')/(' + timeRegex + ')$', "i"),
			function (view, buid, groupid, personid, date, action, minutes) {
				_displayView({ view: view, buid: buid, groupid: groupid, personid: personid, date: date, action: action, minutes: minutes });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, buid, date) {
				_displayView({ view: view, buid: buid, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + actionRegex + ')$', "i"),
			function (view, buid, id, action) {
				_displayView({ view: view, buid: buid, id: id, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, buid, id, date) {
				_displayView({ view: view, buid: buid, id: id, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/#/(' + guidRegex + ')/(' + timeRegex + ')$', "i"),
			function (view, buid, id, date, selectedPersonId, selectedStartMinutes) {
				_displayView({
					view: view,
					buid: buid,
					id: id,
					date: date,
					personid: selectedPersonId,
					selectedStartMinutes: selectedStartMinutes
				});
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + guidRegex + ')$', "i"),
			function (view,buid, id, date, secondaryId) {
				_displayView({ view: view, buid: buid, id: id, date: date, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, buid, groupid, personid, date) {
				_displayView({ view: view, buid: buid, groupid: groupid, personid: personid, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, buid, groupid, personid) {
				_displayView({ view: view, buid: buid, groupid: groupid, personid: personid });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, buid, id) {
				_displayView({ view: view, buid: buid, id: id });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(MultipleTeams)$', "i"),
			function(view,buid, multipleTeams) {
				_displayView({ view: view, buid: buid, id: multipleTeams });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(MultipleSites)$', "i"),
			function(view,buid, multipleTeams) {
				_displayView({ view: view, buid: buid, id: multipleTeams });
			});
		crossroads.addRoute('{view}', function (view) {
			_displayView({ view: view, buid: defaultBu });
		});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, buid) {
				_displayView({ view: view, buid: buid });
		});
		crossroads.addRoute(
			new RegExp('^(' + guidRegex + ')$', "i"),
			function (buid) {
			_displayView({ view: defaultView, buid: buid });
		});
		crossroads.addRoute('', function () {
			_displayView({ view: defaultView, buid: defaultBu });
		});
	}

	function _initializeHasher() {
		var parseHash = function (newHash, oldHash) {
			crossroads.parse(newHash);
		};
		hasher.initialized.add(parseHash);
		hasher.changed.add(parseHash);
		hasher.init();
	}

	function _render() {
		$('body').append(layoutTemplate);
		contentPlaceHolder = $('#content-placeholder');
		$('#menu-placeholder').replaceWith(menuTemplate);
		$('#notification-placeholder').replaceWith(notificationTemplate);
	}

	function _fixBootstrapDropdownForMobileDevices() {
		$('.dropdown-menu').on('touchstart.dropdown.data-api', function (e) {
			e.stopPropagation();
		});
	}

	function _initMomentLanguageWithFallback() {
		var ietfLanguageTag = resources.LanguageCode.toLowerCase();;
		var baseLang = 'en'; //Base
		var languages = [ietfLanguageTag, ietfLanguageTag.split('-')[0], baseLang];

		for (var i = 0; i < languages.length; i++) {
			try {
				moment.lang(languages[i]);
				if (moment.lang() == languages[i]) return;
			} catch (e) {
				continue;
			}
		}
	}

	function bindNotificationViewModel() {
		ko.cleanNode($('#notification-container')[0]);
		ko.applyBindings(notificationsViewModel, $('#notification-container')[0]);
	}

	function trackNotification(personId) {
		trackingmessages.unsubscribeTrackingMessage();
		trackingmessages.subscribeTrackingMessage(menu.CurrentBusinessUnitId(), personId, function (notification) {
			var data = JSON.parse(notification.BinaryData);
			notificationsViewModel.UpdateNotification(notification.DomainId, data.Status);
		}, function () {
		});
	}

	function _bindMenu() {
		ko.cleanNode($('nav')[0]);
		ko.applyBindings(menu, $('nav')[0]);
	}
	
	function _initSignalR() {
		signalrhubs
			.start()
			.fail(function() {
				_displayError("SignalR failed to start");
			});
	}

	var initAll = function() {

		_render();

		_initSignalR();

		_setupRoutes();

		_initializeHasher();

		_initMomentLanguageWithFallback();

		_bindMenu();
	}

	var navigationDataLoadDeferred = $.Deferred();
	var loadNavigationContent = function (callback) {
		ajax.ajax({
			url: "Anywhere/Application/NavigationContent",
			success: callback
		});
	}

	var businessUnitsDataLoadDeffered = $.Deferred();
	var initBusinessUnitsMenu = function (callback) {
		ajax.ajax({
			url: "BusinessUnit",
			success: callback
		});
	};

	var reportsLoadDeffered = $.Deferred();
	var initReportsMenu = function (callback) {
		ajax.ajax({
			url: "Anywhere/Reports/GetReports",
			success: callback
		});
	};

	initReportsMenu(function (data) {
		menu.fillReports(data);
		if (resources.MyTeam_Reports_31070)
			menu.ReportsVisible(menu.reports().length > 0);
		reportsLoadDeffered.resolve();
	});

	initBusinessUnitsMenu(function (data) {
		menu.fillBusinessUnits(data);
		defaultBu = data[0].Id;
		businessUnitsDataLoadDeffered.resolve();
	});

	loadNavigationContent(function(responseData) {
		menu.MyTimeVisible(responseData.IsMyTimeAvailable === true);
		menu.RealTimeAdherenceVisible(responseData.IsRealTimeAdherenceAvailable === true);
		menu.TeamScheduleVisible(responseData.IsTeamScheduleAvailable === true);
		if (!responseData.IsTeamScheduleAvailable && responseData.IsRealTimeAdherenceAvailable) {
			defaultView = 'realtimeadherencesites';
		}
		menu.UserName(responseData.UserName);
		timezoneCurrent.SetIanaTimeZone(responseData.IanaTimeZone);
		trackingPersonId = responseData.PersonId;
		
		navigationDataLoadDeferred.resolve();
	});

	$.when(businessUnitsDataLoadDeffered, navigationDataLoadDeferred).done(function() {
		initAll();
		bindNotificationViewModel();
		trackNotification(trackingPersonId);
	});
});
