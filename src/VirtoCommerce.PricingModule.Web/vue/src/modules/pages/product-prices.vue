<template>
  <!-- A product's price tiers in one price list (legacy prices-list blade) -->
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="60%"
  >
    <VcContainer>
      <div class="pricing-form tw-space-y-4">
        <VcBanner
          v-if="error"
          variant="danger"
          icon="lucide-circle-alert"
        >
          {{ error }}
        </VcBanner>
        <VcCard :header="$t('PRICING.WIDGETS.PRICES')">
          <VcDataTable
            :items="prices"
            edit-mode="inline"
            state-key="PRICING_PRODUCT_PRICES"
            :row-actions="rowActions"
            row-actions-position="column"
            :empty-state="{ icon: 'lucide-badge-dollar-sign', title: $t('PRICING.PRICES.EMPTY') }"
            @cell-edit-complete="onEdit"
          >
            <VcColumn
              id="list"
              :title="$t('PRICING.PRICES.COLUMNS.LIST')"
              type="money"
              editable
              always-visible
            />
            <VcColumn
              id="sale"
              :title="$t('PRICING.PRICES.COLUMNS.SALE')"
              type="money"
              editable
              always-visible
            />
            <VcColumn
              id="minQuantity"
              :title="$t('PRICING.PRICES.COLUMNS.MIN_QTY')"
              type="number"
              editable
              always-visible
            />
            <VcColumn
              id="modifiedDate"
              :title="$t('PRICING.PRICES.COLUMNS.MODIFIED')"
              type="date"
            />
            <!-- hidden by default as in the legacy grid; shown from the column switcher -->
            <VcColumn
              v-for="field in ['startDate', 'endDate'] as const"
              :id="field"
              :key="field"
              :title="$t(field === 'startDate' ? 'PRICING.PRICES.COLUMNS.START' : 'PRICING.PRICES.COLUMNS.END')"
              width="170px"
              :visible="false"
            >
              <!-- the table has no built-in date editor -->
              <template #body="{ index }">
                <VcDatePicker
                  :model-value="prices[index][field]"
                  type="date"
                  clearable
                  @update:model-value="(v: unknown) => (prices[index][field] = (v as Date) || undefined)"
                />
              </template>
            </VcColumn>
          </VcDataTable>
        </VcCard>
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcCard, VcColumn, VcContainer, VcDataTable, VcDatePicker } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { priceError, usePricingClient } from "../api";
import type { Price } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/product-prices", name: "PricingProductPrices", permissions: ["pricing:read"] });

// param: the product id; options: the price list the prices belong to
const { t } = useI18n({ useScope: "global" });
const { param, options, callParent, closeSelf } = useBlade<{ pricelistId: string; pricelistName?: string; currency?: string; productName?: string }>();
const client = usePricingClient();

const title = computed(() => [options.value?.productName, options.value?.pricelistName].filter(Boolean).join(" · "));
const prices = ref<Price[]>([]);
const error = computed(() => prices.value.map((p) => priceError(p, prices.value, t)).find(Boolean) ?? "");

const form = useBladeForm({
  data: prices,
  canSaveOverride: computed(() => !error.value),
  closeConfirmMessage: computed(() => t("PRICING.ALERTS.CLOSE_CONFIRMATION")),
});

function addPrice() {
  prices.value.push({
    productId: param.value,
    pricelistId: options.value?.pricelistId,
    currency: options.value?.currency,
    minQuantity: prices.value.length ? undefined : 1,
  } as Price);
}

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchProductPricesPost({
    priceListIds: [options.value!.pricelistId],
    productIds: [param.value!],
    take: 1000,
  });
  prices.value = (r.results ?? []).flatMap((x) => x.prices ?? []).sort((a, b) => (a.minQuantity ?? 0) - (b.minQuantity ?? 0));
  // opened to price the product in a price list it has no prices in yet: start with one tier
  if (!prices.value.length) addPrice();
  form.setBaseline();
});

function onEdit(e: { field: string; newValue: unknown; index: number }) {
  // an emptied cell (e.g. no sale price) is "not set"
  (prices.value[e.index] as Record<string, unknown>)[e.field] = e.newValue === "" || e.newValue === null ? undefined : Number(e.newValue);
}

// removes the row from the edited list; Save persists it
const rowActions = () => [
  {
    icon: "lucide-trash-2",
    title: t("PRICING.TOOLBAR.DELETE"),
    type: "danger" as const,
    clickHandler: (_: Price, index: number) => prices.value.splice(index, 1),
  },
];

const { loading: saving, action: save } = useAsync(async () => {
  const c = await client();
  if (!prices.value.length) {
    // PUT only replaces prices of the price lists it carries: an emptied list is deleted explicitly
    await c.deleteProductPrices(options.value!.pricelistId, [param.value!]);
    form.setBaseline();
    callParent("reload");
    await closeSelf();
    return;
  }
  await c.updateProductPrices(param.value!, { productId: param.value, prices: prices.value });
  await load();
  callParent("reload");
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("PRICING.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "pricing:update",
    disabled: computed(() => !form.canSave.value || saving.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("PRICING.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "add",
    title: t("PRICING.PRICES.ADD"),
    icon: "lucide-plus",
    permissions: "pricing:update",
    clickHandler: addPrice,
  },
]);

onMounted(load);
</script>
