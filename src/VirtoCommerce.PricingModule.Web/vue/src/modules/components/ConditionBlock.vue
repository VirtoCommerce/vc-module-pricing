<template>
  <!-- "Eligible shoppers" block of a price list assignment: If all/any of the shopper conditions are true -->
  <div class="pricing-conditions">
    <div class="pricing-conditions__row">
      <span>{{ $t("PRICING.CONDITIONS.IF") }}</span>
      <VcSelect
        v-model="block.all"
        class="pricing-conditions__op"
        :options="[
          { id: true, title: $t('PRICING.CONDITIONS.ALL') },
          { id: false, title: $t('PRICING.CONDITIONS.ANY') },
        ]"
        :clearable="false"
        size="small"
      />
      <span>{{ $t("PRICING.CONDITIONS.OF_THESE") }}</span>
    </div>

    <div
      v-if="!block.children?.length"
      class="pricing-conditions__none"
    >
      {{ $t("PRICING.CONDITIONS.NONE") }}
    </div>

    <div
      v-for="(c, i) in block.children"
      :key="i"
      class="pricing-conditions__row pricing-conditions__item"
      :data-condition="c.id"
    >
      <span class="pricing-conditions__label">{{ $t(`PRICING.CONDITIONS.TYPES.${c.id}`) }}</span>
      <VcSelect
        v-if="opsFor(c)"
        v-model="c[opsFor(c)!.field]"
        class="pricing-conditions__op"
        :options="opsFor(c)!.ops.map((id) => ({ id, title: $t(`PRICING.CONDITIONS.OPS.${id}`) }))"
        :clearable="false"
        size="small"
      />
      <VcInput
        v-if="c.id === 'UserGroupsContainsCondition'"
        v-model="c.group"
        class="pricing-conditions__value"
        size="small"
        :placeholder="$t('PRICING.CONDITIONS.VALUE')"
      />
      <VcInput
        v-else-if="c.id === 'ConditionAgeIs'"
        v-model="c.value"
        class="pricing-conditions__value"
        type="number"
        size="small"
      />
      <VcSelect
        v-else-if="lists[c.id!]"
        v-model="c.value"
        class="pricing-conditions__value"
        :options="lists[c.id!]"
        searchable
        :clearable="false"
        size="small"
        :placeholder="$t('PRICING.CONDITIONS.VALUE')"
      />
      <VcInput
        v-else
        v-model="c.value"
        class="pricing-conditions__value"
        size="small"
        :placeholder="$t('PRICING.CONDITIONS.VALUE')"
      />
      <VcButton
        icon="lucide-trash-2"
        text
        :aria-label="$t('PRICING.CONDITIONS.REMOVE')"
        @click="block.children!.splice(i, 1)"
      />
    </div>

    <VcSelect
      :model-value="null"
      class="pricing-conditions__add"
      :options="palette.map((p) => ({ id: p.id, title: $t(`PRICING.CONDITIONS.TYPES.${p.id}`) }))"
      :placeholder="$t('PRICING.CONDITIONS.ADD')"
      :clearable="false"
      size="small"
      @update:model-value="add"
    />
  </div>
</template>

<script setup lang="ts">
import { VcButton, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { http, type Condition } from "../api";

const props = defineProps<{ block: Condition; palette: Condition[] }>();
const { t } = useI18n({ useScope: "global" });

const COMPARE_OPS = ["IsMatching", "IsNotMatching", "IsGreaterThan", "IsGreaterThanOrEqual", "IsLessThan", "IsLessThanOrEqual", "AtLeast"];
const MATCH_OPS = ["Contains", "NotContains", "Matching", "NotMatching"];
// as in the legacy templates, these types pick a single value and show no operator
const NO_OPERATOR = ["ConditionGeoCountry", "ConditionLanguageIs", "ConditionGenderIs"];

function opsFor(c: Condition): { field: "compareCondition" | "matchCondition"; ops: string[] } | undefined {
  if (NO_OPERATOR.includes(c.id!)) return undefined;
  if ("compareCondition" in c) return { field: "compareCondition", ops: COMPARE_OPS };
  if ("matchCondition" in c) return { field: "matchCondition", ops: MATCH_OPS };
  return undefined;
}

type Option = { id: string; title: string };
const countries = ref<Option[]>([]);
const languages = ref<Option[]>([]);
// IANA zones, the same list the legacy UI took from moment-timezone
const timeZones: Option[] = (Intl as unknown as { supportedValuesOf(key: string): string[] })
  .supportedValuesOf("timeZone")
  .map((id) => ({ id, title: id }));
http<{ id: string; name: string }[]>("/api/platform/common/countries").then(
  (x) => (countries.value = x.map((c) => ({ id: c.id, title: c.name }))),
);
http<string[]>("/api/platform/settings/values/VirtoCommerce.Core.General.Languages").then(
  (x) => (languages.value = x.map((id) => ({ id, title: id }))),
);

const lists = computed<Record<string, Option[]>>(() => ({
  ConditionGeoCountry: countries.value,
  ConditionLanguageIs: languages.value,
  ConditionGeoTimeZone: timeZones,
  ConditionGenderIs: ["female", "male"].map((id) => ({ id, title: t(`PRICING.CONDITIONS.GENDERS.${id}`) })),
}));

function add(id: unknown) {
  const prototype = props.palette.find((p) => p.id === id);
  if (!prototype) return;
  (props.block.children ??= []).push(JSON.parse(JSON.stringify(prototype)));
}
</script>

<style lang="scss">
.pricing-conditions {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  font-size: 14px;
  color: var(--color-neutral-700);

  &__row {
    display: flex;
    align-items: center;
    gap: 10px;
  }
  &__item {
    padding-left: 16px;
    border-left: 2px solid var(--color-neutral-200);
  }
  &__label {
    flex: 0 0 150px;
  }
  &__op {
    flex: 0 0 160px;
  }
  &__value {
    flex: 1 1 140px;
    min-width: 0;
  }
  &__add {
    width: 260px;
  }
  &__none {
    color: var(--color-neutral-500);
  }
}
</style>
