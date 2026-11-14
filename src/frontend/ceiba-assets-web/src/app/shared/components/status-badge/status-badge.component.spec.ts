import { TestBed } from '@angular/core/testing';
import { ASSET_STATUSES, ASSET_STATUS_LABELS, AssetStatus } from '../../models/asset.models';
import { StatusBadgeComponent } from './status-badge.component';

describe('StatusBadgeComponent', () => {
  async function render(status: AssetStatus): Promise<HTMLElement> {
    const fixture = TestBed.createComponent(StatusBadgeComponent);
    fixture.componentRef.setInput('status', status);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it.each(ASSET_STATUSES)('muestra la etiqueta en español para %s', async (status) => {
    const element = await render(status);

    expect(element.textContent?.trim()).toBe(ASSET_STATUS_LABELS[status]);
    expect(element.querySelector('.badge')?.getAttribute('data-status')).toBe(status);
  });
});
