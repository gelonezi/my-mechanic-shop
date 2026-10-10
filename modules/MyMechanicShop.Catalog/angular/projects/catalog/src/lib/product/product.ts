import {
  ListService,
  LocalizationPipe,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '@abp/ng.core';
import {
  Confirmation,
  ConfirmationService,
  ModalCloseDirective,
  ModalComponent,
  NgxDatatableDefaultDirective,
  NgxDatatableListDirective,
} from '@abp/ng.theme.shared';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { MyMechanicShop, Products } from '../proxy';

@Component({
  selector: 'lib-product',
  imports: [
    NgxDatatableModule,
    /* ABP's two helper directives for ngx-datatable, both standalone:
     *   [list]  — binds the table to a ListService: server-side paging, sorting and the
     *             loading indicator are wired for you, so no (page)/(sort) handlers here.
     *   default — applies the ABP theme's sizing and column-gap fixes. */
    NgxDatatableListDirective,
    NgxDatatableDefaultDirective,
    /* abp-modal and the [abpClose] directive that dismisses it from the footer button. */
    ModalComponent,
    ModalCloseDirective,
    ReactiveFormsModule,
    LocalizationPipe,
  ],
  templateUrl: './product.html',
  /* ListService is provided PER COMPONENT, not in root: its state is this page's
   * current page/sort/filter. A root-provided one would leak paging between pages. */
  providers: [ListService],
})
export class Product {
  /* Public because the template passes it to [list]. */
  readonly list = inject(ListService);

  /* For the Unit <select>. The generator writes these next to every enum it emits, via
   * ABP's mapEnumToOptions; the template localizes each key as Enum:ProductUnit.<value>.
   * Undefined (0) is left out: the API rejects it, so it must not be offered. */
  protected readonly unitOptions = MyMechanicShop.SharedKernel.Enums.productUnitOptions.filter(
    option => option.value !== MyMechanicShop.SharedKernel.Enums.ProductUnit.Undefined,
  );

  private readonly productsService = inject(Products.ProductsService);
  private readonly confirmation = inject(ConfirmationService);

  /* Declared before `form`, because class fields initialise in source order and
   * form = this.buildForm() runs during construction. */
  private readonly fb = inject(FormBuilder);

  /* A signal, not a plain field. The template reads products() during change detection,
   * so Angular re-renders when .set() runs. A plain field assigned inside subscribe()
   * would not notify Angular under OnPush — the table would silently stay empty. */
  protected readonly products = signal<PagedResultDto<Products.ProductDto>>({
    items: [],
    totalCount: 0,
  });

  /* Drives <abp-modal [(visible)]="isModalOpen">.
   *
   * abp-modal declares `visible` as a ModelSignal (Angular model() input), so the
   * banana-in-a-box binds this WritableSignal directly and Angular writes back with
   * .set() when the user closes the dialog. No (visibleChange) handler needed.
   *
   * It also has to be a signal because save() closes the modal from inside a subscribe
   * callback — an async write, which OnPush would not otherwise notice. */
  protected readonly isModalOpen = signal(false);

  /* The id being edited, or undefined for a new product. This single value decides
   * three things: which endpoint save() calls, what the modal header reads, and whether
   * the form was pre-filled. The tutorial keeps a whole selectedBook object; only the
   * id is actually needed, because editProduct() re-fetches the record anyway.
   *
   * Signal because editProduct() sets it inside a subscribe callback. */
  protected readonly selectedProductId = signal<string | undefined>(undefined);

  /* No type annotation on purpose: the shape is inferred from buildForm(), so the
   * controls stay strongly typed without restating FormGroup<{...}> by hand. */
  protected form = this.buildForm();

  constructor() {
    /* hookToQuery takes a callback, not an Observable: ListService re-invokes it with
     * fresh { skipCount, maxResultCount, sorting } every time the user pages or sorts.
     *
     * Typed as PagedAndSortedResultRequestDto (what getList accepts), not as
     * ABP.PageQueryParams (what hookToQuery declares). The two differ only in that
     * PageQueryParams makes maxResultCount optional; ListService always sets it, so
     * this annotation is the honest one and it keeps the getList call type-checked. */
    const productStreamCreator = (query: PagedAndSortedResultRequestDto) =>
      this.productsService.getList(query);

    this.list.hookToQuery(productStreamCreator).subscribe(response => {
      this.products.set(response);
    });
  }

  /* One builder for both modes. Passing a product pre-fills it for edit; passing nothing
   * gives the empty create form.
   *
   * nonNullable: without it every control is typed string | null, and getRawValue()
   * would not satisfy CreateUpdateProductDto, whose name is required. The ?? fallbacks
   * are needed because every ProductDto member is optional in the proxy.
   *
   * The maxLength values mirror the DTO's [StringLength]s; the server stays the
   * authority (it also checks the EAN digit and rejects Unit = Undefined). */
  private buildForm(product?: Products.ProductDto) {
    const { ProductUnit } = MyMechanicShop.SharedKernel.Enums;
    return this.fb.nonNullable.group({
      name: [product?.name ?? '', [Validators.required, Validators.maxLength(128)]],
      brand: [product?.brand ?? '', Validators.maxLength(128)],
      ean: [product?.ean ?? '', Validators.maxLength(13)],
      description: [product?.description ?? '', Validators.maxLength(1024)],
      unit: [
        product?.unit ?? ProductUnit.Undefined,
        [Validators.required, Validators.min(ProductUnit.Unit)],
      ],
    });
  }

  protected createProduct() {
    this.selectedProductId.set(undefined);
    /* A fresh group rather than reset(), so validators and pristine/touched state start
     * clean every time the dialog opens. */
    this.form = this.buildForm();
    this.isModalOpen.set(true);
  }

  protected editProduct(id: string) {
    /* Re-fetch rather than reuse the row already in the table: the list may have been
     * loaded minutes ago, and GET /{id} is the record as it is right now. It also means
     * the form is built from the same DTO shape in both modes. */
    this.productsService.get(id).subscribe(product => {
      this.selectedProductId.set(product.id);
      this.form = this.buildForm(product);
      this.isModalOpen.set(true);
    });
  }

  protected save() {
    if (this.form.invalid) {
      return;
    }

    /* getRawValue(), not .value: on a typed form .value is Partial<T> because disabled
     * controls drop out of it. getRawValue() returns every control, which is what makes
     * this assignable to CreateUpdateProductDto without a cast. */
    const input = this.form.getRawValue();
    const id = this.selectedProductId();

    /* Both branches produce Observable<ProductDto>, so the subscribe below is shared.
     * Note nothing is sent until subscribe() runs — Angular's HttpClient is cold. */
    const request = id
      ? this.productsService.update(id, input)
      : this.productsService.create(input);

    request.subscribe(() => {
      this.isModalOpen.set(false);
      /* Re-runs the current query through ListService so the change appears without
       * a full page reload — and keeps the current page and sort. */
      this.list.get();
    });
  }

  protected deleteProduct(product: Products.ProductDto) {
    if (!product.id) {
      return;
    }

    const id = product.id;

    /* warn() does not delete anything — it opens ABP's confirmation dialog and emits the
     * user's answer. Anything destructive belongs inside the confirm branch. */
    this.confirmation
      .warn('AbpUi::AreYouSureToDelete', 'AbpUi::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.productsService.delete(id).subscribe(() => this.list.get());
        }
      });
  }
}
