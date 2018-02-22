import { RippleGlobalOptions } from '@angular/material';

/**
 * Disable material ripple and animation to comply with Teleopti styleguide
 * https://github.com/angular/material2/blob/master/src/lib/core/ripple/ripple.md
 */
export const globalRippleConfig: RippleGlobalOptions = {
	disabled: true,
	animation: {
		enterDuration: 0,
		exitDuration: 0
	}
};
