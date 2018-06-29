(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('loginController', loginController, ['$scope', '$http', '$window', '$cookies','$rootScope'])
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
	    var vm = this;
		//checked if has cookie
		var cookie = $cookies.getObject('WfmAdminAuth');
		var token = cookie ? cookie.tokenKey : null;
		var RTA_HangfireStatistics_76139_76373 = false;

	    vm.user = cookie ? cookie.user : null;
		vm.shouldShowEtl = false;
		vm.loginPassword = "";
		vm.Message = '';
		vm.ErrorMessage = '';
		
		vm.Id = cookie ? cookie.id : null;

		$http.get("./Toggle/IsEnabled",
				{
					params: { toggle: "RTA_HangfireStatistics_76139_76373" }
				}, 
				tokenHeaderService.getHeaders())
			.then(function(data) {
				RTA_HangfireStatistics_76139_76373 = data.data;

				$scope.menuItems.push(
					{
						text: "Hangfire Monitoring",
						link: "#/HangfireMonitoring",
						toggle: !RTA_HangfireStatistics_76139_76373
					}
				);

				$scope.menuItems.push(
					{
						text: "Hangfire Statistics",
						link: "#/HangfirePerformanceStats",
						toggle: RTA_HangfireStatistics_76139_76373
					}
				);
			}).then(function() {
				$http.get("./Etl/ShouldEtlToolBeVisible", tokenHeaderService.getHeaders())
					.success(function (data) {
						vm.shouldShowEtl = data;

						$scope.menuItems.push(
							{
								text: "ETL tool",
								link: "#/ETL",
								toggle: vm.shouldShowEtl
							}
						);
					});
			});

	    $scope.state = {
		    selected: 1
	    };
	    $scope.menuItems = [{
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
		    }
	    ];

		$scope.message = "något som jag vill visa";

		$http.get("./HasNoUser").success(function (data) {
			firstUser = data;
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
		}).error(function (xhr, ajaxOptions, thrownError) {
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
			$cookies.putObject('WfmAdminAuth', { 'tokenKey': data.AccessToken, 'user': data.UserName , 'id': data.Id }, { 'expires': expireDate });
		}

		updateCookies();

		function updateCookies() {
			//catch route chnaged : console.log("location changing to:" + next);
			$rootScope.$on("$locationChangeStart", function(event, next, current) {
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
			).success(function (data) {
				$("#modal-login").toggleClass("wait");
				//destory previous cookies
				if (data.Success === false) {
					//alert(data.Message);
					vm.ErrorMessage = data.Message;
					return;
				} else {
					createCookies(data);
					document.location = "#/";
					location.reload();
					//$('#modal-login').dialog('close');
				}

			}).error(showError);
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
