(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('loginController', loginController, ['$scope', '$http', '$window', '$cookies', '$rootScope'])
		.directive('menuItem', function () {
			return {
				scope: {
					text: "@",
					link: "@",
					index: "@",
					state: "="
				},
				templateUrl: 'menuitem.html',
				replace: true,
				bindToController: true,
				controller: function ($scope, $window) {
					this.isSelected = function () {
						return this.state.selected === this.index;
					};
					this.setSelected = function () {
						this.state.selected = this.index;
					};

					if ($window.location.hash === this.link)
						this.setSelected();
				},
				controllerAs: "ctrl"
			};
		});

	function loginController($scope, $http, $cookies, $rootScope, tokenHeaderService) {
		var firstUser = false;
		var isAzure = false;
		var vm = this;
		//checked if has cookie
		var cookie = $cookies.getObject('WfmAdminAuth');
		var token = cookie ? cookie.tokenKey : null;

		vm.azureString = '';
		vm.user = cookie ? cookie.user : null;
		vm.shouldShowEtl = false;
		vm.loginPassword = "";
		vm.Message = '';
		vm.ErrorMessage = '';

		vm.Id = cookie ? cookie.id : null;

		$scope.state = {
			selected: 1
		};
		$scope.menuItems = [
			{
				text: "Tenant administration",
				link: "#/",
				toggle: true
			}, {
				text: "Stardust Dashboard",
				link: "#/StardustDashboard",
				toggle: true
			}, {
				text: "Hangfire Dashboard",
				link: "#/HangfireDashboard",
				toggle: true
			}, {
				text: "Hangfire Statistics",
				link: "#/HangfireStatistics",
				toggle: true
			},{
				text: "ETL",
				link: "#/ETL",
				toggle: true
			}
		];

		$scope.message = "något som jag vill visa";

		$http.get("./HasNoUser")
			.then(function (response) {
				firstUser = response.data;
				if (firstUser) {
					vm.user = 'xxfirstxx';
					window.location = "firstuser.html";
				} else {
					if (!token) {
						$("#modal-login").dialog({
							modal: true,
							title: "Log in to access the admin site",
							closeOnEscape: false,
							draggable: false,
							resizable: false
						});
					}
				}
			}).catch(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});

		$http.get("./isAzure")
			.then(function (response) {
				isAzure = response.data;
				if (isAzure) {
					vm.azureString = ' (Azure)';
				}
			}).catch(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});

		function showError(jqXHR) {
			vm.ErrorMessage = jqXHR.Message + ': ' + jqXHR.ExceptionMessage;
		}

		function createCookies(data) {
			vm.user = data.UserName;
			vm.Id = data.Id;
			vm.Message = 'Successful log in...';
			// Cache the username token in session storage.
			vm.UserName = data.UserName;

			//lets do authentication in cookie
			var today = new Date();
			var expireDate = new Date(today.getTime() + 30 * 60000);
			$cookies.putObject('WfmAdminAuth', { 'tokenKey': data.AccessToken, 'user': data.UserName, 'id': data.Id }, { 'expires': expireDate });
		}

		updateCookies();

		function updateCookies() {
			//catch route chnaged : console.log("location changing to:" + next);
			$rootScope.$on("$locationChangeStart", function (event, next, current) {
				//check cookies
				var checkedCookie = $cookies.getObject('WfmAdminAuth');
				if (!checkedCookie) {
					return;
				} else {
					//update cookies
					var info = $cookies.getObject('WfmAdminAuth');
					var today = new Date();
					var newExpireDate = new Date(today.getTime() + 30 * 60000);
					$cookies.putObject('WfmAdminAuth', info, { 'expires': newExpireDate });
				}
			});
		}

		vm.login = function () {
			vm.Id = 0;
			vm.ErrorMessage = '';
			$("#modal-login").toggleClass("wait");
			var loginData = {
				granttype: 'password',
				username: vm.loginEmail,
				password: vm.loginPassword
			};

			$http.post('./Login',
				loginData
			).then(function (response) {
				$("#modal-login").toggleClass("wait");
				//destory previous cookies
				if (response.data.Success === false) {
					vm.ErrorMessage = response.data.Message;
					return;
				} else {
					createCookies(response.data);
					document.location = "#/";
					location.reload();
				}

			}).catch(showError);
		};

		vm.logout = function () {
			$cookies.remove('WfmAdminAuth');
			$http.post('./Logout');
			vm.user = null;
			document.location = "#/";
			location.reload();
		};
	}

})();
