var webdriverio = require('webdriverio');
var request = require("request");
var client = webdriverio
	.remote({
		desiredCapabilities: {
			browserName: 'chrome'
		}
	});
	
var log = function(msg){
	client.call(function(){console.log(msg)});
};
client.init();
log('navigate to web');
client.url('https://teleoptirnd.teleopticloud.com/Web')
	.waitForExist('#Username-input', 60000, false, function(err, res, response) {
		if (err) {
			log('failed to navigate to sign in page.');
			throw ('failed to navigate to sign in page.');
		}
	});
log('try to sign in');
client.setValue('#Username-input', 'demo')
	.setValue('#Password-input', 'demo')
	.click('#Signin-button')
	.waitForExist('.user-name', 60000, false, function(err, res, response) {
		if (err) {
			log('failed to sign in.');
			throw ('failed to sign in.');
		}
		log('sign in succeeded');
	});
log('navigate to health check');
client.url('https://teleoptirnd.teleopticloud.com/Web/HealthCheck')
	.waitForEnabled('#Start-Check', 60000, false, function(err, res, response) {
		if (err) {
			log('failed to navigate to health check page.');
			throw ('failed to navigate to health check page.');
		}
	});
log('check service bus and broker');
client.pause(1000);
client.click('#Start-Check')
	.waitForExist('#Bus-Results', 300000, false, function(err, res, response) {
		if (err) {
			log('service bus doesnot work well after trying 5 minutes.');
			throw ('service bus doesnot work well.');
		}
		log('service bus and broker work well');
	});
log('shutdown client.');
client.end();
log('shutdown selenium server');
client.call(function(){
	request('http://localhost:4444/selenium-server/driver/?cmd=shutDownSeleniumServer');
});