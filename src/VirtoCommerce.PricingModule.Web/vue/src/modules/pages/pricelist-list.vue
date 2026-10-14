<template>
  <VcBlade
    :title="$t('PRICING.PRICELISTS.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
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
      state-key="PRICING_PRICELISTS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :global-filters="globalFilters"
      :empty-state="{ icon: 'lucide-badge-dollar-sign', title: $t('PRICING.PRICELISTS.EMPTY') }"
      @row-click="(e: { data: Pricelist }) => openDetails(e.data)"
      @pagination-click="pagination.goToPage"
      @filter="onFilter"
    >
      <VcColumn
        id="name"
        :title="$t('PRICING.PRICELISTS.COLUMNS.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="currency"
        :title="$t('PRICING.PRICELISTS.COLUMNS.CURRENCY')"
        sortable
        always-visible
      />
      <VcColumn
        id="description"
        :title="$t('PRICING.PRICELISTS.COLUMNS.DESCRIPTION')"
      />
      <VcColumn
        id="modifiedDate"
        :title="$t('PRICING.PRICELISTS.COLUMNS.MODIFIED')"
        type="date"
        sortable
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
import { exportOptions, getCurrencies, useExportAvailable, usePricingClient } from "../api";
import type { Pricelist } from "../../api_client/virtocommerce.pricing";

defineBlade({
  url: "/pricing",
  name: "PricingPricelistList",
  isWorkspace: true,
  permissions: ["pricing:access"],
  menuItem: { title: "PRICING.MENU.TITLE", icon: "lucide-badge-dollar-sign", priority: 20 },
});

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren } = useBlade();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const client = usePricingClient();
const canExport = useExportAvailable();

const items = ref<Pricelist[]>([]);
const totalCount = ref(0);
const selection = ref<Pricelist[]>([]);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "PRICING_PRICELISTS", initialField: "name", initialDirection: "ASC" });
const { searchValue } = useTableSearch({ stateKey: "PRICING_PRICELISTS" });
const pagination = useDataTablePagination({ stateKey: "PRICING_PRICELISTS", pageSize: PAGE_SIZE, totalCount });

// currency filter (legacy: the currencies of an export saved filter)
const currencies = ref<string[]>([]);
const currencyFilter = ref<string[]>();
getCurrencies().then((x) => (currencies.value = x.map((c) => c.code)));
const globalFilters = computed(() => [
  {
    id: "currencies",
    label: t("PRICING.PRICELISTS.COLUMNS.CURRENCY"),
    filter: { options: currencies.value.map((c) => ({ value: c, label: c })), multiple: true },
  },
]);
function onFilter(event: { filters: Record<string, unknown> }) {
  const value = event.filters.currencies;
  currencyFilter.value = value ? ([] as string[]).concat(value as string[]) : undefined;
  // back to page 1 reloads through the paging watcher
  if (pagination.skip) pagination.setPage(1);
  else load();
}

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchPricelists(
    currencyFilter.value?.length ? currencyFilter.value : undefined, "NoDetails", undefined, undefined, undefined, searchValue.value || undefined,
    undefined, undefined, sortExpression.value, undefined, pagination.skip, PAGE_SIZE,
  );
  items.value = r.results ?? [];
  totalCount.value = r.totalCount ?? 0;
});

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

function openDetails(item?: Pricelist) {
  activeItemId.value = item?.id;
  openBlade({
    name: "PricingPricelistDetails",
    param: item?.id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("PRICING.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("PRICING.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "pricing:create",
    clickHandler: () => openDetails(),
  },
  {
    id: "delete",
    title: t("PRICING.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "pricing:delete",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("PRICING.ALERTS.DELETE_PRICELISTS", { count: selection.value.length })))) return;
      await closeChildren();
      if (activeItemId.value) return; // the open record kept its unsaved edits: nothing deleted
      await (await client()).deletePricelists(selection.value.map((x) => x.id!));
      selection.value = [];
      await load();
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
          "VirtoCommerce.PricingModule.Data.ExportImport.ExportablePricelist",
          {
            exportTypeName: "PricelistExportDataQuery",
            // nothing selected: everything the current search and filter match
            isAllSelected: !selection.value.length,
            objectIds: selection.value.map((x) => x.id!),
            keyword: searchValue.value || undefined,
            sort: sortExpression.value,
            currencies: currencyFilter.value,
          },
          selection.value.length || totalCount.value,
          "PRICING.EXPORT.PRICELISTS_TITLE",
          "PRICING.EXPORT.PRICELISTS_SUBTITLE",
        ),
      }),
  },
  {
    id: "assignments",
    title: t("PRICING.TOOLBAR.ASSIGNMENTS"),
    icon: "lucide-anchor",
    clickHandler: () => openBlade({ name: "PricingAssignmentList" }),
  },
]);

exposeToChildren({ reload: () => load() });
</script>
