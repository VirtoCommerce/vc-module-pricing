<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="pricing-form tw-space-y-4">
        <VcInput
          v-model="item.name"
          :label="$t('PRICING.PRICELISTS.FIELDS.NAME')"
          :placeholder="$t('PRICING.PRICELISTS.PLACEHOLDERS.NAME')"
          required
        />
        <VcSelect
          v-model="item.currency"
          :label="$t('PRICING.PRICELISTS.FIELDS.CURRENCY')"
          :placeholder="$t('PRICING.PRICELISTS.PLACEHOLDERS.CURRENCY')"
          :options="currencies"
          option-value="code"
          option-label="code"
          searchable
          required
          :clearable="false"
        />
        <VcTextarea
          v-model="item.description"
          :label="$t('PRICING.PRICELISTS.FIELDS.DESCRIPTION')"
          :placeholder="$t('PRICING.PRICELISTS.PLACEHOLDERS.DESCRIPTION')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, useBladeWidgets, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect, VcTextarea } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getCurrencies, usePricingClient } from "../api";
import type { Pricelist } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/pricelist", name: "PricingPricelistDetails", permissions: ["pricing:read"] });

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, replaceWith, exposeToChildren } = useBlade();
const client = usePricingClient();

const item = ref<Pricelist>({});
const currencies = ref<{ code: string }[]>([]);
const pricesCount = ref<number | string>("…");
const assignmentsCount = ref<number | string>("…");
const isNew = computed(() => !param.value);
const title = computed(() => (isNew.value ? t("PRICING.PRICELISTS.NEW") : (item.value.name ?? "")));

const form = useBladeForm({
  data: item,
  canSaveOverride: computed(() => !!item.value.name?.trim() && !!item.value.currency),
  closeConfirmMessage: computed(() => t("PRICING.ALERTS.CLOSE_CONFIRMATION")),
});

const { loading, action: load } = useAsync(async () => {
  const c = await client();
  item.value = await c.getPriceListInShortById(param.value!);
  form.setBaseline();
  refreshCounts();
});

async function refreshCounts() {
  if (isNew.value) return;
  const c = await client();
  const [prices, assignments] = await Promise.all([
    c.searchProductPricesPost({ priceListIds: [param.value!], groupByProducts: true, take: 0 }),
    c.searchPricelistAssignments(param.value!, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, 0),
  ]);
  pricesCount.value = prices.totalCount ?? 0;
  assignmentsCount.value = assignments.totalCount ?? 0;
}

const { loading: saving, action: save } = useAsync(async () => {
  const c = await client();
  if (isNew.value) {
    const created = await c.createPriceList(item.value);
    form.setBaseline();
    callParent("reload");
    // reopen as an existing price list so its widgets (prices, assignments) become available
    await replaceWith({ name: "PricingPricelistDetails", param: created.id });
    return;
  }
  await c.updatePriceList(item.value);
  await load();
  callParent("reload");
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => t(isNew.value ? "PRICING.TOOLBAR.CREATE" : "PRICING.TOOLBAR.SAVE")),
    icon: "lucide-save",
    permissions: isNew.value ? "pricing:create" : "pricing:update",
    disabled: computed(() => !form.canSave.value || saving.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("PRICING.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
]);

useBladeWidgets([
  {
    id: "PricingPricelistPrices",
    icon: "lucide-badge-dollar-sign",
    title: "PRICING.PRICELISTS.WIDGETS.PRICES",
    badge: pricesCount,
    isVisible: computed(() => !isNew.value),
    onClick: () =>
      openBlade({
        name: "PricingPricelistProducts",
        param: item.value.id,
        options: { pricelist: item.value },
        onClose: refreshCounts,
      }),
    onRefresh: refreshCounts,
  },
  {
    id: "PricingPricelistAssignments",
    icon: "lucide-anchor",
    title: "PRICING.PRICELISTS.WIDGETS.ASSIGNMENTS",
    badge: assignmentsCount,
    isVisible: computed(() => !isNew.value),
    onClick: () =>
      openBlade({
        name: "PricingAssignmentList",
        param: item.value.id,
        options: { pricelist: item.value },
        onClose: refreshCounts,
      }),
    onRefresh: refreshCounts,
  },
]);

onMounted(async () => {
  const currenciesLoaded = getCurrencies().then((x) => (currencies.value = x));
  if (isNew.value) {
    await currenciesLoaded;
    item.value = { currency: currencies.value[0]?.code };
    form.setBaseline();
  } else {
    await Promise.all([currenciesLoaded, load()]);
  }
});

exposeToChildren({ refreshCounts });
</script>

<style lang="scss">
/* the design's blade body padding, as in the Orders details */
.pricing-form {
  padding: 20px 22px 36px;
}
</style>
