<template>
  <!-- A product's prices in every price list (opened from the Prices widget of the Catalog product blade).
       One row per price list; its tiers are edited in PricingProductPrices. -->
  <VcBlade
    :title="$t('PRICING.PRICES.ITEM_TITLE', { name: productName })"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <div class="pricing-add-price">
      <VcSelect
        v-model="newPricelistId"
        class="pricing-add-price__select"
        :label="$t('PRICING.PRICES.SELECT_PRICELIST')"
        :options="pricelists"
        option-value="id"
        option-label="displayName"
        searchable
      />
      <VcButton
        icon="lucide-plus"
        variant="outline"
        :disabled="!newPricelistId"
        @click="openPrices(newPricelistId!)"
      >
        {{ $t("PRICING.PRICES.ADD") }}
      </VcButton>
    </div>
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      state-key="PRICING_ITEM_PRICES"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-badge-dollar-sign', title: $t('PRICING.PRICES.EMPTY') }"
      @row-click="(e: { data: Row }) => openPrices(e.data.id!)"
    >
      <VcColumn
        id="name"
        :title="$t('PRICING.PRICES.COLUMNS.PRICELIST')"
        always-visible
      >
        <template #body="{ data }">
          <a
            class="pricing-link"
            @click.stop="openBlade({ name: 'PricingPricelistDetails', param: data.id })"
          >{{ data.name }}</a>
        </template>
      </VcColumn>
      <VcColumn
        id="currency"
        :title="$t('PRICING.PRICES.COLUMNS.CURRENCY')"
        always-visible
      />
      <VcColumn
        id="range"
        :title="$t('PRICING.PRODUCTS.COLUMNS.RANGE')"
        align="end"
        always-visible
      >
        <template #body="{ data }">
          <span class="vc-table-cell-money">{{ data.range }}</span>
        </template>
      </VcColumn>
      <VcColumn
        id="count"
        :title="$t('PRICING.PRODUCTS.COLUMNS.PRICES')"
        align="end"
      />
      <VcColumn
        id="catalog"
        :title="$t('PRICING.PRICES.COLUMNS.CATALOG')"
        :visible="false"
      />
      <VcColumn
        id="store"
        :title="$t('PRICING.PRICES.COLUMNS.STORE')"
        :visible="false"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcColumn, VcDataTable, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { loadNames, priceRange, usePricingClient } from "../api";
import type { Pricelist } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/product-pricing", name: "PricingItemPrices", permissions: ["pricing:read"] });

type Row = Pricelist & { range: string; count: number; catalog: string; store: string };

// param: the product id
const { t, n } = useI18n({ useScope: "global" });
const { param, options, openBlade, exposeToChildren } = useBlade<{ productName?: string }>();
const { showConfirmation } = usePopup();
const client = usePricingClient();

const productName = computed(() => options.value?.productName ?? "");
const pricelists = ref<(Pricelist & { displayName: string })[]>([]);
const rows = ref<Row[]>([]);
const selection = ref<Row[]>([]);
const activeItemId = ref<string>();
const newPricelistId = ref<string>();
// catalog and store names (assignments carry ids only): read once, not on every reload
const names = Promise.all([loadNames("/api/catalog/catalogs/search"), loadNames("/api/stores/search")]);

const { loading, action: load } = useAsync(async () => {
  const [lists, [catalogs, stores]] = await Promise.all([(await client()).getProductPriceLists(param.value!), names]);
  const fmt = (v: number) => n(v, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  const nameList = (ids: (string | undefined)[], map: Record<string, string>) =>
    ids.filter((id): id is string => !!id).map((id) => map[id] ?? id).join(", ");
  pricelists.value = lists.map((x) => ({ ...x, displayName: `${x.name} - ${x.currency}` }));
  newPricelistId.value ??= pricelists.value[0]?.id;
  // as the legacy grid: each price list next to the catalogs and stores it is assigned to
  rows.value = lists
    .filter((x) => x.prices?.length)
    .map((x) => ({
      ...x,
      range: priceRange(x.prices!, fmt),
      count: x.prices!.length,
      catalog: nameList(x.assignments?.map((a) => a.catalogId) ?? [], catalogs),
      store: nameList(x.assignments?.map((a) => a.storeId) ?? [], stores),
    }));
  selection.value = [];
});

function openPrices(pricelistId: string) {
  const pl = pricelists.value.find((x) => x.id === pricelistId);
  activeItemId.value = pricelistId;
  openBlade({
    name: "PricingProductPrices",
    param: param.value,
    options: { pricelistId, pricelistName: pl?.name, currency: pl?.currency, productName: productName.value },
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("PRICING.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "delete",
    title: t("PRICING.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "pricing:delete",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      const count = selection.value.reduce((sum, x) => sum + x.count, 0);
      if (!(await showConfirmation(t("PRICING.ALERTS.DELETE_PRICES", { count })))) return;
      const ids = selection.value.flatMap((x) => x.prices!.map((p) => p.id!));
      await (await client()).deleteProductPrice(ids);
      await load();
    },
  },
]);

onMounted(load);
exposeToChildren({ reload: () => load() });
</script>

<style lang="scss">
.pricing-add-price {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  padding: 16px 20px 8px;
  &__select {
    flex: 1;
    min-width: 0;
  }
}
.pricing-link {
  color: var(--color-primary-500);
  cursor: pointer;
  &:hover {
    text-decoration: underline;
  }
}
</style>
