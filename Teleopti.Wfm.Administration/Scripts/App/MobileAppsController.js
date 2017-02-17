(function() {
	angular.module('adminApp').controller('MobileAppsController',['$q','$http', MobileAppsController]);

	 function MobileAppsController($q, $http) {
		var vm = this;

		vm.inputUrl = "http://localhost:52858/mytime#Schedule/Week";
		vm.teleoptiAndroidAppLink = "https://play.google.com/store/apps/details?id=com.teleopti.mobile";
		vm.teleoptiIOSAppLink = "https://itunes.apple.com/";
		vm.showAndriodAppQRCode = false;
		vm.showIOSAppQRCode = false;

		vm.saveBaseUrl = function(){
			console.log(vm.inputUrl);
		};
	}
})();