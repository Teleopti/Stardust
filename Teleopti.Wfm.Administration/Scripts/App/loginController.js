(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('loginController', loginController, ['$scope', '$http', '$rootScope'])
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

	function loginController($scope, $http, $rootScope) {
		var firstUser = false;
		var isAzure = false;
		var vm = this;
		vm.user = null;
		vm.id = null;

		vm.azureString = '';
		vm.shouldShowEtl = false;
		vm.loginPassword = "";
		vm.Message = '';
		vm.ErrorMessage = '';
		$rootScope.user = "";

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

		$http.get("./HasNoUser")
			.then(function (response) {
				firstUser = response.data;
				if (firstUser) {
					vm.user = '';
					$rootScope.user = "";
					window.location = "firstuser.html";
				} else {
					$http.get("./LoggedInUser")
						.then(function(response) {
							if (response.data.Name !== null && response.data.Name !== '') {
								//vm.user = response.data.Name;
								$rootScope.user = response.data.Name;
								vm.id = response.data.Id;
							} else {
								vm.user = "";
								$rootScope.user = "";
								vm.id = null;
								$("#modal-login").dialog({
									modal: true,
									title: "Log in to access the admin site",
									closeOnEscape: false,
									draggable: false,
									resizable: false
								});
							}
						}).catch(function (xhr, ajaxOptions, thrownError) {
							console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
						});
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

		function saveUser(data) {
			$rootScope.user = data.UserName;
			vm.user = data.UserName;
			vm.id = data.Id;
			vm.Message = 'Successful log in...';
		}

		vm.login = function () {
			vm.id = null;
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
				if (response.data.Success === false) {
					vm.ErrorMessage = response.data.Message;
					return;
				} else {
					saveUser(response.data);
					document.location = "#/";
					location.reload();
				}

			}).catch(showError);
		};

		vm.logout = function () {
			$http.post('./Logout');
			vm.user = '';
			vm.id = null;
			document.location = "#/";
			location.reload();
		};
	}

})();
