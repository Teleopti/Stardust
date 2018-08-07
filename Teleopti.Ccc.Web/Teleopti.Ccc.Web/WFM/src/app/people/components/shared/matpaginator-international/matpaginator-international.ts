import { MatPaginatorIntl } from '@angular/material';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class MatPaginatorInternational extends MatPaginatorIntl {
	translate: TranslateService;

	itemsPerPageLabel: string;
	nextPageLabel: string;
	previousPageLabel: string;

	getRangeLabel = function(page: number, pageSize: number, length: number) {
		const ofText = this.translate ? this.translate.instant('OfText') : 'of';
		if (length === 0 || pageSize === 0) {
			return '0 ' + ofText + ' ' + length;
		}

		return page * pageSize + 1 + ' - ' + (page * pageSize + pageSize) + ' ' + ofText + ' ' + length;
	};

	injectTranslateService(translate: TranslateService) {
		this.translate = translate;

		this.translate.onLangChange.subscribe(() => {
			this.translateLabels();
		});

		this.translateLabels();
	}

	translateLabels() {
		this.itemsPerPageLabel = this.translate.instant('ItemsPerPage');
		this.nextPageLabel = this.translate.instant('NextPage');
		this.previousPageLabel = this.translate.instant('PreviousPage');
	}
}
