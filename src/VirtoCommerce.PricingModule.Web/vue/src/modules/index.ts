import { defineAppModule, registerExternalWidget, useExtensionPoint } from "@vc-shell/framework";
import { markRaw } from "vue";
import * as blades from "./pages";
import * as locales from "./locales";
import ProductPricesWidget from "./components/ProductPricesWidget.vue";
import ExportFilterField from "./components/ExportFilterField.vue";

// Prices widget on the Catalog product blade (legacy: itemPricesWidget in the "itemDetail" widget group)
registerExternalWidget({
  id: "PricingProductPrices",
  title: "Prices",
  component: markRaw(ProductPricesWidget),
  targetBlades: ["CatalogProductDetails"],
});

// Filter fields of Pricing's export types in the Export module's saved-filter blade (legacy metaFormsService
// "<type>ExportFilter" registrations)
const EXPORT_FILTERS: Record<string, string[]> = {
  ExportablePricelist: ["currencies"],
  ExportablePrice: ["pricelistIds", "productIds", "modifiedSince"],
  ExportablePricelistAssignment: ["pricelistIds", "catalogIds"],
};
Object.entries(EXPORT_FILTERS).forEach(([type, fields]) =>
  fields.forEach((field, i) =>
    useExtensionPoint(`export-filter:VirtoCommerce.PricingModule.Data.ExportImport.${type}`).add({
      id: `pricing-${field}`,
      component: markRaw(ExportFilterField),
      props: { field },
      priority: i,
    }),
  ),
);

export default defineAppModule({
  blades,
  locales,
});
