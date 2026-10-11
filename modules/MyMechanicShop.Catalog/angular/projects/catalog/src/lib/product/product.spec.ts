import { DefaultQueueManager, PermissionService, QUEUE_MANAGER } from '@abp/ng.core';
import { CoreTestingModule, wait } from '@abp/ng.core/testing';
import { of } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Product } from './product';

/* CoreTestingModule is ABP's own harness: it pulls in BaseCoreModule (so RestService,
 * ListService and the CORE_OPTIONS token resolve) plus a LocalizationPipe that returns the
 * key unchanged. That last part is why the assertions below match on keys such as
 * 'Catalog::Brand' rather than on translated text — no localization JSON is loaded here,
 * and asserting translations is not this test's job.
 *
 * MockPermissionService replaces the real one so the *abpPermission buttons can be driven
 * deterministically: grantAllPolicies() for the happy path, nothing granted to prove the
 * gating actually hides them. */
describe('Product', () => {
  let fixture: ComponentFixture<Product>;
  let httpMock: HttpTestingController;

  /* One catalog row carrying every field, including the optional ones, so the table
   * assertions below are about rendering rather than about the API. */
  const productRow = {
    id: '11111111-1111-1111-1111-111111111111',
    name: 'Spark plug',
    brand: 'NGK',
    ean: '4006381333931',
    description: 'Iridium spark plug.',
    unit: 1,
  };

  async function setup(grantAll = true) {
    await TestBed.configureTestingModule({
      /* listQueryDebounceTime: 0 — ListService debounces its query by default, so with the
       * stock value the list request has not been issued yet when the test tries to flush
       * it, and the table renders headers with no rows. */
      imports: [
        Product,
        CoreTestingModule.withConfig({
          listQueryDebounceTime: 0,
          /* The generated service declares apiName 'Catalog', and RestService resolves that
           * through EnvironmentService. Without an entry here it cannot build a URL, so no
           * request is ever issued and the table stays empty. */
          environment: {
            apis: {
              default: { url: '' },
              Catalog: { url: '' },
            },
          },
        }),
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        /* A flat stub rather than ABP's MockPermissionService: the mock keeps its answer in
         * a ConfigStateService store, which made the result depend on what an earlier test
         * had granted. PermissionDirective only reads getGrantedPolicy$, so answering that
         * directly makes each case deterministic. */
        {
          provide: PermissionService,
          useValue: {
            getGrantedPolicy$: () => of(grantAll),
            getGrantedPolicy: () => grantAll,
            filterItemsByPolicy: (items: unknown[]) => items,
            filterItemsByPolicy$: (items: unknown[]) => of(items),
          },
        },
        /* CoreTestingModule brings in BaseCoreModule, not RootCoreModule, so the
         * root-only providers are absent. *abpPermission injects a QueueManager, which
         * is one of them — without this the component cannot be constructed at all. */
        { provide: QUEUE_MANAGER, useClass: DefaultQueueManager },
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Product);
    fixture.detectChanges();

    /* ListService fires the list request from the constructor. Flushing it here is what
     * puts a row in the table; without it the component renders headers only. */
    await wait(fixture);
    httpMock
      .match(req => req.url.includes('/api/catalog/products'))
      .forEach(req => req.flush({ totalCount: 1, items: [productRow] }));

    /* *abpPermission resolves its policy through a QueueManager, so the button appears a
     * macrotask later than the first render — ABP's own wait() helper covers that. */
    await wait(fixture);
    fixture.detectChanges();
  }

  afterEach(() => TestBed.resetTestingModule());

  it('should create', async () => {
    await setup();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders a column for every listed field', async () => {
    await setup();

    /* The testing LocalizationPipe returns the key with the resource prefix stripped, so
     * 'Catalog::Brand' renders as 'Brand'. Asserting on the header cells rather than the
     * whole document keeps 'Name' from matching text elsewhere on the page. */
    const headers = Array.from(
      fixture.nativeElement.querySelectorAll('.datatable-header-cell'),
    ).map(el => ((el as HTMLElement).textContent ?? '').trim());

    // Description is intentionally absent from the table: it is long text, edited in the modal.
    expect(headers).toEqual(expect.arrayContaining(['Name', 'Brand', 'Ean', 'Unit']));
  });

  it('renders the value objects of a row, and the unit through its localization key', async () => {
    await setup();

    /* Driving the row through the signal the template reads, rather than through a flushed
     * HTTP response: what is under test is the template's bindings, not ListService or
     * RestService. Going through HTTP here would make a rendering test depend on URL
     * resolution and the query debounce as well. */
    fixture.componentInstance['products'].set({ totalCount: 1, items: [productRow] });
    fixture.detectChanges();
    await wait(fixture);

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('Spark plug');
    expect(text).toContain('NGK');
    expect(text).toContain('4006381333931');
    // Proves the cell template binds row.unit, not a hard-coded label. The prefix is
    // stripped by the testing pipe, leaving the key's own suffix.
    expect(text).toContain('Enum:ProductUnit.1');
  });

  it('builds a form control for all five product fields', async () => {
    await setup();
    const form = fixture.componentInstance['form'];

    expect(Object.keys(form.controls).sort()).toEqual([
      'brand',
      'description',
      'ean',
      'name',
      'unit',
    ]);
  });

  it('offers every unit except Undefined, which the API rejects', async () => {
    await setup();
    const units = fixture.componentInstance['unitOptions'];

    expect(units.length).toBeGreaterThan(0);
    expect(units.some((o: { value: number }) => o.value === 0)).toBe(false);
  });

  it('hides the create button when the permission is not granted', async () => {
    await setup(/* grantAll */ false);

    expect(fixture.nativeElement.querySelector('#create')).toBeNull();
  });

  it('shows the create button when the permission is granted', async () => {
    await setup(/* grantAll */ true);

    expect(fixture.nativeElement.querySelector('#create')).not.toBeNull();
  });
});
