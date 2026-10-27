<template>
  <!-- One field of a Pricing export filter (legacy metafields "<type>ExportFilter"), rendered by the Export module's
       filter blade at the extension point "export-filter:<exportTypeName>"; writes its criterion onto `filter` -->
  <VcDatePicker
    v-if="field === 'modifiedSince'"
    v-model="model"
    type="datetime-local"
    :label="$t('PRICING.EXPORT.FILTER.modifiedSince')"
    clearable
  />
  <VcSelect
    v-else
    v-model="model"
    :label="$t(`PRICING.EXPORT.FILTER.${field}`)"
    :options="sources[field as Exclude<Field, 'modifiedSince'>]"
    option-value="id"
    option-label="name"
    multiple
    searchable
  />
</template>

<script setup lang="ts">
import { VcDatePicker, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { getCurrencies, searchCatalogs, searchPricelistOptions, searchProductOptions, type Named } from "../api";

type Field = "currencies" | "pricelistIds" | "productIds" | "catalogIds" | "modifiedSince";
const props = defineProps<{ filter: Record<string, unknown>; field: Field }>();

const model = computed({
  get: () => props.filter[props.field] as never,
  set: (value: unknown) => {
    props.filter[props.field] = (Array.isArray(value) && !value.length) || value === null ? undefined : value;
  },
});

type Source = Named[] | ((keyword?: string, skip?: number, ids?: string[]) => Promise<{ results: Named[]; totalCount: number }>);
const currencies = ref<Named[]>([]);
if (props.field === "currencies") getCurrencies().then((x) => (currencies.value = x.map((c) => ({ id: c.code, name: c.code }))));
const sources = computed<Record<Exclude<Field, "modifiedSince">, Source>>(() => ({
  currencies: currencies.value,
  pricelistIds: searchPricelistOptions,
  productIds: searchProductOptions,
  catalogIds: searchCatalogs,
}));
</script>
