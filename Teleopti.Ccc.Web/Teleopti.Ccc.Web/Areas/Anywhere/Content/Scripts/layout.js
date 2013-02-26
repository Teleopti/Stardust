
require([
		'text!templates/layout.html',
		'text!templates/menu.html',
		'text!templates/dummy.html',
		'crossroads',
		'hasher',
		'knockout',
		'moment',
		'momentDatepickerKo',
		'menu',
		'noext!../../../../signalr/hubs',
		'noext!application/resources'
	], function (
		layoutTemplate,
		menuTemplate,
		dummyTemplate,
		crossroads,
		hasher,
		ko,
		moment,
		datepicker,
		menuViewModel,
		signalrHubs,
		resources) {

		var currentView;
		var defaultView = 'teamschedule';
		var menu = new menuViewModel(resources);
		var startedPromise;

		function _displayView(routeInfo) {

			var placeHolder = $('#content-placeholder');
			placeHolder.html(dummyTemplate).find("h2").text(routeInfo.view);
			routeInfo.renderHtml = function (html) {
				placeHolder.html(html);
			};

			routeInfo.startedPromise = startedPromise;

			if (currentView && currentView.dispose)
				currentView.dispose();

			var module = 'views/' + routeInfo.view + '/view';
			require([module], function (view) {
				currentView = view;
				view.display(routeInfo);
				_fixBootstrapDropdownForMobileDevices();
			});

			menu.ActiveView(routeInfo.view);
		}

		function _setupRoutes() {
			var viewRegex = '[a-z]+';
			var actionRegex = '[a-z]+';
			var guidRegex = '[a-z0-9]{8}(?:-[a-z0-9]{4}){3}-[a-z0-9]{12}';
			var dateRegex = '\\d{8}';

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
				function (view, id, date, action) {
					_displayView({ view: view, id: id, date: date, action: action });
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
			$('#menu-placeholder').replaceWith(menuTemplate);
		}

		function _fixBootstrapDropdownForMobileDevices() {
			$('.dropdown-menu').on('touchstart.dropdown.data-api', function (e) {
				e.stopPropagation();
			});
		}

		function _initMomentLanguageWithFallback() {
			var ietfLanguageTag = resources.LanguageCode;
			var baseLang = 'en'; //Base
			var languages = [ietfLanguageTag, ietfLanguageTag.split('-')[0], baseLang];

			for (var i = 0; i < languages.length; i++) {
				try {
					moment.lang(languages[i]);
				} catch (e) {
					continue;
				}
				if (moment.lang() == languages[i]) return;
			}
		}

		function _bindMenu() {
			$.ajax({
				dataType: "json",
				cache: false,
				url: "Application/NavigationContent",
				success: function (responseData, textStatus, jqXHR) {
					menu.MyTimeVisible(responseData.IsMyTimeAvailable === true);
					menu.MobileReportsVisible(responseData.IsMobileReportsAvailable === true);
					menu.UserName(responseData.UserName);
				}
			});
			ko.applyBindings(menu, $('nav')[0]);
		}

		function _initSignalR() {
			$.connection.hub.url = 'signalr';
			startedPromise = $.connection.hub.start();
			startedPromise.fail(function() {
				$('.container > .row:first').html('<div class="alert"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>Warning!</strong> ' + error + '.</div>');
			});
		}

		_render();

		_initSignalR();

		_setupRoutes();
		_initializeHasher();

		_initMomentLanguageWithFallback();

		_bindMenu();
	});
