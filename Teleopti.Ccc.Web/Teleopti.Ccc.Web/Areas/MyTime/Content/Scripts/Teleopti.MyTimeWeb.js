if (typeof Teleopti === 'undefined') {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === 'undefined') {
	Teleopti.MyTimeWeb = {};
}

// Hide all tooltips when touchstart, scroll or orientationchange
document.addEventListener(
	'touchstart',
	function() {
		$('.tooltip').hide();
	},
	true
);
$('.pagebody').on('scroll', function() {
	$('.tooltip').hide();
});
$(window).on('orientationchange', function() {
	$('.tooltip').hide();
});
