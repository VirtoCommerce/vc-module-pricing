<template>
  <VcBlade
    :title="$t('PRICING.PRODUCTS.TITLE', { name: pricelist?.name ?? '' })"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="PRICING_PRICELIST_PRODUCTS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-badge-dollar-sign', title: $t('PRICING.PRODUCTS.EMPTY') }"
      @row-click="(e: { data: ProductPrice }) => openPrices(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="image"
        field="product.imgSrc"
        :title="$t('PRICING.PRODUCTS.COLUMNS.IMAGE')"
        type="image"
        width="80px"
      />
      <VcColumn
        id="name"
        field="product.name"
        :title="$t('PRICING.PRODUCTS.COLUMNS.NAME')"
        always-visible
      >
        <template #body="{ data }">{{ data.product?.name ?? $t("PRICING.PRODUCTS.NO_PRODUCT") }}</template>
      </VcColumn>
      <VcColumn
        id="code"
        field="product.code"
        :title="$t('PRICING.PRODUCTS.COLUMNS.CODE')"
      />
      <VcColumn
        id="count"
        :title="$t('PRICING.PRODUCTS.COLUMNS.PRICES')"
        align="end"
      >
        <template #body="{ data }">{{ data.prices?.length ?? 0 }}</template>
      </VcColumn>
      <VcColumn
        id="range"
        :title="$t('PRICING.PRODUCTS.COLUMNS.RANGE')"
        align="end"
        always-visible
      >
        <template #body="{ data }">
          <span class="vc-table-cell-money">{{ range(data) }}</span>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
/**
 * Extension point "pricing-pricelist-products-toolbar": other modules add toolbar commands to this list
 * (legacy: toolbarService.register(..., "virtoCommerce.pricingModule.pricelistItemListController"), e.g. CSV import/export).
 *
 *   useExtensionPoint("pricing-pricelist-products-toolbar").add({
 *     id: "my-command",
 *     priority: 10,                               // order among the added commands
 *     component: markRaw({ render: () => null }), // required by the framework type, not rendered
 *     meta: {
 *       title: "MYMODULE.COMMAND",                // i18n key or text
 *       icon: "lucide-upload",
 *       permissions: "pricing:simpleImport",      // optional, string or string[] (any of)
 *       isVisible: (ctx) => true,                 // optional
 *       disabled: (ctx) => !ctx.selection.length, // optional
 *       onClick: (ctx) => ctx.openBlade({ name: "MyBlade", options: { pricelistId: ctx.pricelistId } }),
 *     } satisfies PricingProductsToolbarCommand,
 *   });
 *
 * ctx (PricingProductsToolbarContext):
 *   pricelistId  the price list id
 *   pricelist    the price list (Pricelist), once loaded
 *   selection    the selected rows (ProductPrice groups: productId, product, prices)
 *   keyword      the list's search keyword, undefined when empty
 *   totalCount   the number of rows in the list (all pages)
 *   openBlade    opens a blade as a child of this list
 *   reload       reloads the list
 */
import {
  defineExtensionPoint,
  useAsync,
  useBlade,
  useDataTablePagination,
  useFunctions,
  usePermissions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { exportOptions, priceRange, useExportAvailable, usePricingClient, type ProductEntry } from "../api";
import type { Pricelist, ProductPrice } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/pricelist-products", name: "PricingPricelistProducts", permissions: ["pricing:read"] });

export interface PricingProductsToolbarContext {
  pricelistId: string;
  pricelist?: Pricelist;
  selection: ProductPrice[];
  keyword?: string;
  totalCount: number;
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  reload: () => Promise<void>;
}
export interface PricingProductsToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: PricingProductsToolbarContext) => boolean;
  disabled?: (ctx: PricingProductsToolbarContext) => boolean;
  onClick: (ctx: PricingProductsToolbarContext) => unknown;
}
const extensions = defineExtensionPoint<PricingProductsToolbarCommand>("pricing-pricelist-products-toolbar", {
  description: "Toolbar commands of the price list products list",
});

const PAGE_SIZE = 20;
const { t, n } = useI18n({ useScope: "global" });
const { param, options, openBlade, closeChildren, callParent, exposeToChildren } = useBlade<{ pricelist: Pricelist }>();
const { hasAccess } = usePermissions();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const client = usePricingClient();
const canExport = useExportAvailable();

// options don't survive a page reload; the price list is then re-read
const pricelist = ref(options.value?.pricelist);
if (!pricelist.value) client().then(async (c) => (pricelist.value = await c.getPriceListInShortById(param.value!)));
const items = ref<ProductPrice[]>([]);
const totalCount = ref(0);
const selection = ref<ProductPrice[]>([]);
const activeItemId = ref<string>();
const { searchValue } = useTableSearch({ stateKey: "PRICING_PRICELIST_PRODUCTS" });
const pagination = useDataTablePagination({ stateKey: "PRICING_PRICELIST_PRODUCTS", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchProductPricesPost({
    priceListIds: [param.value!],
    groupByProducts: true,
    keyword: searchValue.value || undefined,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });
  // the table keys rows by id: a product-price group is identified by its product
  items.value = (r.results ?? []).map((x) => ({ ...x, id: x.productId }) as ProductPrice);
  totalCount.value = r.totalCount ?? 0;
});
// prices were added or deleted: the price list's counts change too (paging and search leave them as they are)
async function reload() {
  await load();
  callParent("refreshCounts");
}

load();
watch(searchValue, () => pagination.setPage(1));
watch([searchValue, () => pagination.skip], debounce(load, 300));

const range = (group: ProductPrice) =>
  priceRange(group.prices ?? [], (v) => n(v, { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

function openPrices(group: ProductPrice) {
  activeItemId.value = group.productId;
  openBlade({
    name: "PricingProductPrices",
    param: group.productId,
    options: {
      pricelistId: param.value,
      currency: pricelist.value?.currency,
      productName: group.product?.name ?? t("PRICING.PRODUCTS.NO_PRODUCT"),
    },
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

// products picked in PricingProductPicker get a zero list price (legacy behavior), edited afterwards
async function addProducts(products: ProductEntry[]) {
  const c = await client();
  const existing = await c.searchProductPricesPost({ priceListIds: [param.value!], productIds: products.map((p) => p.id), take: 1000 });
  const known = new Set((existing.results ?? []).map((x) => x.productId));
  const added = products.filter((p) => !known.has(p.id));
  if (added.length) {
    await c.updateProductsPrices(
      added.map((p) => ({
        productId: p.id,
        prices: [{ productId: p.id, list: 0, minQuantity: 1, currency: pricelist.value?.currency, pricelistId: param.value }],
      })),
    );
  }
  await reload();
}

const ownToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("PRICING.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => reload() },
  {
    id: "add",
    title: t("PRICING.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "pricing:update",
    // the picker searches the catalog (catalog:products:read falls back to catalog:read)
    isVisible: hasAccess(["catalog:read", "catalog:products:read"]),
    clickHandler: () => openBlade({ name: "PricingProductPicker" }),
  },
  {
    id: "delete",
    title: t("PRICING.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "pricing:update",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("PRICING.ALERTS.DELETE_PRODUCT_PRICES", { count: selection.value.length })))) return;
      await closeChildren();
      if (activeItemId.value) return; // the open record kept its unsaved edits: nothing deleted
      await (await client()).deleteProductPrices(param.value!, selection.value.map((x) => x.productId!));
      selection.value = [];
      await reload();
    },
  },
  {
    id: "export",
    title: t("PRICING.TOOLBAR.EXPORT"),
    icon: "lucide-file-output",
    permissions: "pricing:export",
    isVisible: canExport,
    clickHandler: () =>
      openBlade({
        name: "ExportSettings",
        options: exportOptions(
          "VirtoCommerce.PricingModule.Data.ExportImport.ExportablePrice",
          {
            exportTypeName: "PriceExportDataQuery",
            // nothing selected: every price of this price list the current search matches
            isAllSelected: !selection.value.length,
            objectIds: selection.value.flatMap((x) => (x.prices ?? []).map((p) => p.id!)),
            productIds: selection.value.map((x) => x.productId!),
            pricelistIds: [param.value],
            keyword: searchValue.value || undefined,
          },
          selection.value.reduce((sum, x) => sum + (x.prices?.length ?? 0), 0) || undefined,
          "PRICING.EXPORT.PRICES_TITLE",
          "PRICING.EXPORT.PRICES_SUBTITLE",
        ),
      }),
  },
]);

const toolbarContext = (): PricingProductsToolbarContext => ({
  pricelistId: param.value!,
  pricelist: pricelist.value,
  selection: selection.value,
  keyword: searchValue.value || undefined,
  totalCount: totalCount.value,
  openBlade,
  reload,
});

const bladeToolbar = computed<IBladeToolbar[]>(() => {
  const ctx = toolbarContext();
  return [
    ...ownToolbar.value,
    // commands other modules add (extension point "pricing-pricelist-products-toolbar")
    ...extensions.components.value
      .filter((x) => x.meta && (!x.meta.isVisible || x.meta.isVisible(ctx)))
      .map((x) => ({
        id: x.id,
        title: t(x.meta!.title),
        icon: x.meta!.icon,
        permissions: x.meta!.permissions,
        disabled: !!x.meta!.disabled?.(ctx),
        // a fresh context at click time
        clickHandler: () => x.meta!.onClick(toolbarContext()),
      })),
  ];
});

exposeToChildren({ reload, addProducts });
</script>
