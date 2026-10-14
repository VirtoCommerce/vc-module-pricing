<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="60%"
  >
    <VcContainer>
      <VcForm class="pricing-form tw-space-y-4">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.NAME')"
              :placeholder="$t('PRICING.ASSIGNMENTS.PLACEHOLDERS.NAME')"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.pricelistId"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.PRICELIST')"
              :placeholder="$t('PRICING.ASSIGNMENTS.PLACEHOLDERS.PRICELIST')"
              :options="searchPricelistOptions"
              option-value="id"
              option-label="name"
              searchable
              required
              :clearable="false"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="item.storeId"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.STORE')"
              :placeholder="$t('PRICING.ASSIGNMENTS.PLACEHOLDERS.STORE')"
              :options="searchStores"
              option-value="id"
              option-label="name"
              searchable
              :disabled="!!item.catalogId"
              :error="!targetValid"
              :error-message="targetValid ? undefined : $t('PRICING.VALIDATION.ASSIGNMENT_TARGET')"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.catalogId"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.CATALOG')"
              :placeholder="$t('PRICING.ASSIGNMENTS.PLACEHOLDERS.CATALOG')"
              :options="searchCatalogs"
              option-value="id"
              option-label="name"
              searchable
              :disabled="!!item.storeId"
              :hint="$t('PRICING.ASSIGNMENTS.FIELDS.TARGET_HINT')"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.priority"
              type="number"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.PRIORITY')"
              :hint="$t('PRICING.ASSIGNMENTS.FIELDS.PRIORITY_HINT')"
              required
            />
          </VcCol>
          <VcCol />
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcDatePicker
              v-model="item.startDate"
              type="datetime-local"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.START')"
              :hint="$t('PRICING.ASSIGNMENTS.FIELDS.START_HINT')"
              clearable
            />
          </VcCol>
          <VcCol>
            <VcDatePicker
              v-model="item.endDate"
              type="datetime-local"
              :label="$t('PRICING.ASSIGNMENTS.FIELDS.END')"
              :hint="$t('PRICING.ASSIGNMENTS.FIELDS.END_HINT')"
              clearable
            />
          </VcCol>
        </VcRow>
        <VcTextarea
          v-model="item.description"
          :label="$t('PRICING.ASSIGNMENTS.FIELDS.DESCRIPTION')"
          :placeholder="$t('PRICING.ASSIGNMENTS.PLACEHOLDERS.DESCRIPTION')"
        />
        <VcCard
          v-if="block"
          :header="$t('PRICING.CONDITIONS.TITLE')"
        >
          <ConditionBlock
            :block="block"
            :palette="palette"
          />
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCard,
  VcCol,
  VcContainer,
  VcDatePicker,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { searchCatalogs, searchPricelistOptions, searchStores, usePricingClient, type Condition } from "../api";
import ConditionBlock from "../components/ConditionBlock.vue";
import type { PricelistAssignment } from "../../api_client/virtocommerce.pricing";

defineBlade({ url: "/pricing-assignment", name: "PricingAssignmentDetails", permissions: ["pricing:read"] });

const { t } = useI18n({ useScope: "global" });
const { param, options, callParent, replaceWith } = useBlade<{ pricelistId?: string }>();
const client = usePricingClient();

const item = ref<PricelistAssignment>({});
const palette = ref<Condition[]>([]);
const isNew = computed(() => !param.value);
const title = computed(() => (isNew.value ? t("PRICING.ASSIGNMENTS.NEW") : (item.value.name ?? "")));
// tree shape: PriceConditionTree -> one BlockPricingCondition (all/any) -> shopper conditions
const block = computed(() => item.value.dynamicExpression?.children?.[0] as Condition | undefined);
// exactly one of store / catalog
const targetValid = computed(() => !item.value.storeId !== !item.value.catalogId);

const form = useBladeForm({
  data: item,
  canSaveOverride: computed(
    () => !!item.value.name?.trim() && !!item.value.pricelistId && targetValid.value && item.value.priority !== undefined && (item.value.priority as unknown) !== "",
  ),
  closeConfirmMessage: computed(() => t("PRICING.ALERTS.CLOSE_CONFIRMATION")),
});

// the server prototype lists the condition types an assignment supports (legacy: availableChildren)
function stripUi(node: Condition): Condition {
  const { availableChildren: _, ...rest } = node;
  return { ...rest, children: (node.children ?? []).map(stripUi) };
}

// the prototype is read once per blade (a save reloads only the assignment), alongside the assignment
let prototype: Promise<PricelistAssignment> | undefined;
const { loading, action: load } = useAsync(async () => {
  const c = await client();
  prototype ??= c.getNewPricelistAssignments().catch((e) => {
    prototype = undefined;
    throw e;
  });
  const [proto, existing] = await Promise.all([prototype, isNew.value ? undefined : c.getPricelistAssignmentById(param.value!)]);
  palette.value = ((proto.dynamicExpression?.availableChildren?.[0] as Condition)?.availableChildren ?? []).map(stripUi);
  item.value = existing ?? { ...proto, pricelistId: options.value?.pricelistId };
  if (item.value.dynamicExpression) item.value.dynamicExpression = stripUi(item.value.dynamicExpression as Condition);
  form.setBaseline();
});

const { loading: saving, action: save } = useAsync(async () => {
  const c = await client();
  item.value.priority = Number(item.value.priority);
  if (isNew.value) {
    const created = await c.createPricelistAssignment(item.value);
    form.setBaseline();
    callParent("reload");
    await replaceWith({ name: "PricingAssignmentDetails", param: created.id });
    return;
  }
  await c.updatePriceListAssignment(item.value);
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

onMounted(load);
</script>
