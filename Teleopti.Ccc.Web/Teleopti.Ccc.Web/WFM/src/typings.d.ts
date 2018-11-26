import * as _angular_ from 'angular';
import * as _moment_with_timezone_ from 'moment-timezone';

import * as _d3_ from 'd3';
/* SystemJS module definition */
declare var module: NodeModule;
interface NodeModule {
	id: string;
}

/* angularjs global definition */
declare global {
	const angular: typeof _angular_;
	const moment: typeof _moment_with_timezone_;
	const d3: typeof _d3_;
}
