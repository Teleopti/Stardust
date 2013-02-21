
require([
		'text!templates/layout.html',
		'text!templates/menu.html',
		'text!templates/dummy.html',
		'crossroads',
		'hasher',
		'knockout',
		'momentDatepickerKo',
		'noext!application/resources'
	], function (
		layoutTemplate,
		menuTemplate,
		dummyTemplate,
		crossroads,
		hasher,
		ko,
		datepicker,
		translations) {

		var currentView;
		var defaultView = 'teamschedule';

		var navigationViewModel = {
			Translations: translations,
			MyTimeVisible: ko.observable(false),
			MobileReportsVisible: ko.observable(false)
		};

		function _displayView(routeInfo) {

			var placeHolder = $('#content-placeholder');
			placeHolder.html(dummyTemplate).find("h2").text(routeInfo.view);
			routeInfo.renderHtml = function (html) {
				placeHolder.html(html);
			};

			if (currentView && currentView.dispose)
				currentView.dispose();

			var module = 'views/' + routeInfo.view + '/view';
			require([module], function (view) {
				currentView = view;
				view.display(routeInfo);
				_fixBootstrapDropdownForMobileDevices();
			});

			var navList = $('.nav > li');
			navList.removeClass('active');
			navList.each(function () {
				if ($(this).attr('class')) return;
				var href = $(this).children('a').attr('href');
				if (href === '#') {
					href = defaultView;
				}
				if (href.indexOf(routeInfo.view) > -1) {
					$(this).addClass('active');
					return;
				}
			});
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

		function _updateMenu() {
			$.getJSON('Application/NavigationContent?' + $.now()).success(function (responseData, textStatus, jqXHR) {
				if (responseData.IsMyTimeAvailable)
					navigationViewModel.MyTimeVisible(true);
				if (responseData.IsMobileReportsAvailable)
					navigationViewModel.MobileReportsVisible(true);

				$('#username').text(responseData.UserName);
			});
		}

		function _fixBootstrapDropdownForMobileDevices() {
			$('.dropdown-menu').on('touchstart.dropdown.data-api', function (e) {
				e.stopPropagation();
			});
		}

		function _bindViewModel() {
			ko.applyBindings(navigationViewModel, $('nav')[0]);
		}

		_render();
		_updateMenu();
		_setupRoutes();
		_initializeHasher();
		_bindViewModel();
	});
