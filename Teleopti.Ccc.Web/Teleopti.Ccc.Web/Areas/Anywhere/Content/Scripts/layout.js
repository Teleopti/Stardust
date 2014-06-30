
define([
		'text!templates/layout.html',
		'text!templates/menu.html',
		'text!templates/error.html',
		'crossroads',
		'hasher',
		'knockout',
		'moment',
		'momentDatepickerKo',
		'menu',
		'subscriptions',
		'ajax',
		'errorview',
		'resources'
], function (
		layoutTemplate,
		menuTemplate,
		errorTemplate,
		crossroads,
		hasher,
		ko,
		moment,
		datepicker,
		menuViewModel,
		subscriptions,
		ajax,
		errorview,
		resources) {

	var currentView;
	var defaultView = 'teamschedule';
	
	var menu = new menuViewModel(resources);
	var permissions= {};
	var contentPlaceHolder;

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

			if (view != currentView) {
				if (currentView && currentView.dispose)
					currentView.dispose(routeInfo);
				currentView = view;
				routeInfo.permissions = permissions;
				view.initialize(routeInfo);
			}

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

		menu.ActiveView(routeInfo.view);
	}

	function _setupRoutes() {
		var viewRegex = '[a-z]+';
		var actionRegex = '[a-z]+';
		var guidRegex = '[a-z0-9]{8}(?:-[a-z0-9]{4}){3}-[a-z0-9]{12}';
		var dateRegex = '\\d{8}';
		var timeRegex = '\\d*';

		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, id, date, action, secondaryId) {
				_displayView({ view: view, id: id, date: date, action: action, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + actionRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, id, action, secondaryId) {
				_displayView({ view: view, id: id, action: action, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')$', "i"),
			function (view, personid, date, action) {
				_displayView({ view: view, personid: personid, date: date, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')$', "i"),
			function (view, groupid, personid, date, action) {
				_displayView({ view: view, groupid: groupid, personid: personid, date: date, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + actionRegex + ')/(' + timeRegex + ')$', "i"),
			function (view, groupid, personid, date, action, minutes) {
				_displayView({ view: view, groupid: groupid, personid: personid, date: date, action: action, minutes: minutes });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, date) {
				_displayView({ view: view, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + actionRegex + ')$', "i"),
			function (view, id, action) {
				_displayView({ view: view, id: id, action: action });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, id, date) {
				_displayView({ view: view, id: id, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, id, date, secondaryId) {
				_displayView({ view: view, id: id, date: date, secondaryId: secondaryId });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')/(' + guidRegex + ')/(' + dateRegex + ')$', "i"),
			function (view, groupid, personid, date) {
				_displayView({ view: view, groupid: groupid, personid: personid, date: date });
			});
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + guidRegex + ')$', "i"),
			function (view, id) {
				_displayView({ view: view, id: id });
			});
		crossroads.addRoute('{view}', function (view) {
			_displayView({ view: view });
		});
		crossroads.addRoute('', function () {
			_displayView({ view: defaultView });
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

	function _bindMenu() {
		ajax.ajax({
			url: "Anywhere/Application/NavigationContent",
			success: function (responseData, textStatus, jqXHR) {
				menu.MyTimeVisible(responseData.IsMyTimeAvailable === true);
				menu.RealTimeAdherenceVisible(responseData.IsRealTimeAdherenceAvailable === true);
				menu.UserName(responseData.UserName);
			}
		});
		ko.applyBindings(menu, $('nav')[0]);
	}

	function _loadPermissions() {
		ajax.ajax({
			url: "Anywhere/Application/Permissions",
			success: function (responseData, textStatus, jqXHR) {
				permissions.addFullDayAbsence = responseData.IsAddFullDayAbsenceAvailable;
				permissions.addIntradayAbsence = responseData.IsAddIntradayAbsenceAvailable;
			}
		});
	}

	function _initSignalR() {
		var promise = subscriptions.start();
		promise.fail(function () {
			_displayError("SignalR failed to start");
		});
	}


		_render();

		_initSignalR();

		_setupRoutes();
		_initializeHasher();

		_initMomentLanguageWithFallback();

		_bindMenu();

		_loadPermissions();

});
