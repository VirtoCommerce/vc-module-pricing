<template>
  <!-- Prices widget of the Catalog product blade (legacy itemPricesWidget): price range, opens the product's prices -->
  <VcWidget
    v-if="hasAccess('pricing:read')"
    icon="lucide-badge-dollar-sign"
    :title="$t('PRICING.WIDGETS.PRICES')"
    :value="label"
    :disabled="!product?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { priceRange, usePricingClient } from "../api";
import type { Price } from "../../api_client/virtocommerce.pricing";

// the product shared by the Catalog blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const product = computed(() => ctx.value.item as { id?: string; catalogId?: string; name?: string } | undefined);
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();
const { n } = useI18n({ useScope: "global" });
const client = usePricingClient();
const label = ref<string>("…");

async function refresh() {
  const { id, catalogId } = product.value ?? {};
  if (!id || !catalogId) return;
  const prices = await (await client()).evaluateProductPricesForCatalog(id, catalogId);
  if (!prices.length) {
    label.value = "N/A";
    return;
  }
  // the currency with the most prices, as the legacy widget does
  const byCurrency = new Map<string, Price[]>();
  prices.forEach((p) => byCurrency.set(p.currency ?? "", [...(byCurrency.get(p.currency ?? "") ?? []), p]));
  const group = [...byCurrency.values()].reduce((a, b) => (b.length > a.length ? b : a));
  const fmt = (v: number) => n(v, { style: "currency", currency: group[0].currency || "USD" });
  label.value = priceRange(group, fmt) || "N/A";
}

// the product blade replaces its item on save and reset: re-evaluate only when the product or catalog changes
watch([() => product.value?.id, () => product.value?.catalogId], refresh, { immediate: true });

function open() {
  if (!product.value?.id) return;
  openBlade({
    name: "PricingItemPrices",
    param: product.value.id,
    options: { productName: product.value.name },
    onClose: refresh,
  });
}
</script>
