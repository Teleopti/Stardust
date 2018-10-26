import { Injectable } from '@angular/core';

type Version = string;

@Injectable()
export class VersionService {
	private version = '';

	getVersion(): Version {
		return this.version;
	}

	setVersion(version: Version) {
		this.version = version;
	}
}
