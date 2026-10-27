<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="60%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="PRICING_ASSIGNMENTS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-anchor', title: $t('PRICING.ASSIGNMENTS.EMPTY') }"
      @row-click="(e: { data: PricelistAssignment }) => openDetails(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="target"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.CATALOG') + ' / ' + $t('PRICING.ASSIGNMENTS.COLUMNS.STORE')"
        always-visible
      >
        <template #body="{ data }">{{ data.catalogId ? catalogs[data.catalogId] ?? data.catalogId : stores[data.storeId] ?? data.storeId }}</template>
      </VcColumn>
      <VcColumn
        id="pricelistName"
        field="pricelist.name"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.PRICELIST')"
      />
      <VcColumn
        id="currency"
        field="pricelist.currency"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.CURRENCY')"
      />
      <VcColumn
        id="priority"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.PRIORITY')"
        type="number"
        sortable
      />
      <VcColumn
        id="startDate"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.START')"
        type="date"
        sortable
      />
      <VcColumn
        id="endDate"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.END')"
        type="date"
        sortable
      />
      <VcColumn
        id="modifiedDate"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.MODIFIED')"
        type="date"
        sortable
        :visible="false"
      />
      <VcColumn
        id="modifiedBy"
        :title="$t('PRICING.ASSIGNMENTS.COLUMNS.MODIFIED_BY')"
        :visible="false"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { exportOptions, loadNames, useExportAvailable, usePricingClient } from "../api";
import type { Pricelist, PricelistAssignment } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/pricing-assignments", name: "PricingAssignmentList", permissions: ["pricing:read"] });

// param: optional price list id; without it the blade lists the assignments of all price lists
const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { param, options, openBlade, closeChildren, callParent, exposeToChildren } = useBlade<{ pricelist?: Pricelist }>();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const client = usePricingClient();
const canExport = useExportAvailable();

const title = computed(() =>
  options.value?.pricelist?.name
    ? `${options.value.pricelist.name}: ${t("PRICING.ASSIGNMENTS.TITLE")}`
    : t("PRICING.ASSIGNMENTS.TITLE"),
);
const items = ref<PricelistAssignment[]>([]);
const totalCount = ref(0);
const selection = ref<PricelistAssignment[]>([]);
const activeItemId = ref<string>();
const catalogs = ref<Record<string, string>>({});
const stores = ref<Record<string, string>>({});

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "PRICING_ASSIGNMENTS", initialField: "priority", initialDirection: "DESC" });
const { searchValue } = useTableSearch({ stateKey: "PRICING_ASSIGNMENTS" });
const pagination = useDataTablePagination({ stateKey: "PRICING_ASSIGNMENTS", pageSize: PAGE_SIZE, totalCount });

const priceListIds = computed(() => (param.value ? [param.value] : undefined));

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchPricelistAssignments(
    undefined, undefined, undefined, priceListIds.value, undefined, undefined, undefined, undefined,
    searchValue.value || undefined, undefined, undefined, sortExpression.value, undefined, pagination.skip, PAGE_SIZE,
  );
  items.value = r.results ?? [];
  totalCount.value = r.totalCount ?? 0;
});
// assignments were added or deleted: the price list's counts change too (paging and search leave them as they are)
async function reload() {
  await load();
  callParent("refreshCounts");
}

// catalog and store names for the target column (assignments carry ids only)
loadNames("/api/catalog/catalogs/search").then((x) => (catalogs.value = x));
loadNames("/api/stores/search").then((x) => (stores.value = x));
load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

function openDetails(item?: PricelistAssignment) {
  activeItemId.value = item?.id;
  openBlade({
    name: "PricingAssignmentDetails",
    param: item?.id,
    options: item ? undefined : { pricelistId: param.value },
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("PRICING.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => reload() },
  { id: "add", title: t("PRICING.TOOLBAR.ADD"), icon: "lucide-plus", permissions: "pricing:create", clickHandler: () => openDetails() },
  {
    id: "delete",
    title: t("PRICING.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "pricing:delete",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("PRICING.ALERTS.DELETE_ASSIGNMENTS", { count: selection.value.length })))) return;
      await closeChildren();
      if (activeItemId.value) return; // the open record kept its unsaved edits: nothing deleted
      await (await client()).deleteAssignments(selection.value.map((x) => x.id!));
      selection.value = [];
      await reload();
    },
  },
  {
    id: "deleteAll",
    title: computed(() => t(searchValue.value ? "PRICING.TOOLBAR.DELETE_FILTERED" : "PRICING.TOOLBAR.DELETE_ALL")),
    icon: "lucide-trash",
    permissions: "pricing:delete",
    disabled: computed(() => !totalCount.value),
    async clickHandler() {
      const count = totalCount.value;
      if (!(await showConfirmation(t("PRICING.ALERTS.DELETE_ALL_ASSIGNMENTS", { count })))) return;
      await closeChildren();
      if (activeItemId.value) return; // the open record kept its unsaved edits: nothing deleted
      // the server deletes one search page (default take 20): ask for the whole confirmed count
      await (await client()).deleteFilteredAssignments(
        undefined, undefined, undefined, priceListIds.value, undefined, undefined, undefined, undefined, searchValue.value || undefined,
        undefined, undefined, undefined, undefined, 0, count,
      );
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
          "VirtoCommerce.PricingModule.Data.ExportImport.ExportablePricelistAssignment",
          {
            exportTypeName: "PricelistAssignmentExportDataQuery",
            // nothing selected: everything the current search (and price list) match
            isAllSelected: !selection.value.length,
            objectIds: selection.value.map((x) => x.id!),
            keyword: searchValue.value || undefined,
            sort: sortExpression.value,
            pricelistIds: priceListIds.value ?? [],
          },
          selection.value.length || totalCount.value,
          "PRICING.EXPORT.ASSIGNMENTS_TITLE",
          "PRICING.EXPORT.ASSIGNMENTS_SUBTITLE",
        ),
      }),
  },
]);

exposeToChildren({ reload });
</script>
