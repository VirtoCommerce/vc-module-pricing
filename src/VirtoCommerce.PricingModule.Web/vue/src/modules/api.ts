import { getBladeConfig, ModulesReadyKey, useApiClient, usePermissions } from "@vc-shell/framework";
import { computed, inject, ref } from "vue";
import { PricingModuleClient, type Price } from "../api_client/virtocommerce.pricing";

export function usePricingClient() {
  return useApiClient(PricingModuleClient).getApiClient;
}

// Other modules' endpoints this UI reads (catalogs, stores, currencies, products): a few calls,
// so plain same-origin fetches instead of generating their whole API clients.
export async function http<T>(url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method: body === undefined ? "GET" : "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}: ${url}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export interface Named {
  id: string;
  name: string;
}

type SearchResult<T> = { results?: T[]; totalCount?: number };

// VcSelect async option sources: (keyword, skip, ids) => { results, totalCount }
function selectSource(url: string) {
  return async (keyword?: string, skip = 0, ids?: string[]) => {
    const r = await http<SearchResult<Named>>(url, { keyword, objectIds: ids, skip, take: ids?.length || 20 });
    return { results: r.results ?? [], totalCount: r.totalCount ?? 0 };
  };
}
export const searchCatalogs = selectSource("/api/catalog/catalogs/search");

export async function searchPricelistOptions(keyword?: string, skip = 0, ids?: string[]) {
  const q = new URLSearchParams({ responseGroup: "NoDetails", sort: "name", skip: String(skip), take: String(ids?.length || 20) });
  if (keyword) q.set("keyword", keyword);
  ids?.forEach((id) => q.append("objectIds", id));
  const r = await http<SearchResult<Named>>(`/api/pricing/pricelists?${q}`);
  return { results: r.results ?? [], totalCount: r.totalCount ?? 0 };
}
export const searchStores = selectSource("/api/stores/search");

export async function loadNames(url: string): Promise<Record<string, string>> {
  const r = await http<SearchResult<Named>>(url, { take: 1000 });
  return Object.fromEntries((r.results ?? []).map((x) => [x.id, x.name]));
}

export const getCurrencies = () => http<{ code: string; name?: string }[]>("/api/currencies");

export interface ProductEntry {
  id: string;
  name: string;
  code?: string;
  imageUrl?: string;
  catalogId?: string;
}

export async function searchProducts(keyword: string | undefined, skip: number, take: number) {
  const r = await http<{ listEntries?: ProductEntry[]; totalCount?: number }>("/api/catalog/listentries", {
    keyword,
    objectTypes: ["CatalogProduct"],
    searchInChildren: true,
    searchInVariations: true,
    skip,
    take,
  });
  return { results: r.listEntries ?? [], totalCount: r.totalCount ?? 0 };
}

export async function searchProductOptions(keyword?: string, skip = 0, ids?: string[]) {
  if (ids?.length) {
    const items = await http<ProductEntry[]>("/api/catalog/products/plenty?respGroup=ItemInfo", ids);
    return { results: items ?? [], totalCount: items?.length ?? 0 };
  }
  return searchProducts(keyword, skip, 20);
}

// --- price rules (legacy priceValidatorsService) -----------------------------------------------

const isSingleItemPrice = (p: Price) => Math.round(Number(p.minQuantity)) === 1 && !p.startDate && !p.endDate;

/** Validation message for one price row among `all` prices of the same product, or "" when valid. */
export function priceError(p: Price, all: Price[], t: (key: string) => string): string {
  const list = p.list as unknown;
  if (list === undefined || list === null || list === "" || Number(list) < 0) return t("PRICING.VALIDATION.LIST");
  if (p.sale !== undefined && p.sale !== null && (p.sale as unknown) !== "" && Number(p.sale) > Number(list))
    return t("PRICING.VALIDATION.SALE");
  const qty = Math.round(Number(p.minQuantity));
  if (!(qty > 0)) return t("PRICING.VALIDATION.QTY");
  const samePricelist = all.filter((x) => x.pricelistId === p.pricelistId);
  // quantity tiers must be unique (prices limited by dates are exempt)
  if (
    !p.startDate &&
    !p.endDate &&
    samePricelist.some((x) => x !== p && !x.startDate && !x.endDate && Math.round(Number(x.minQuantity)) === qty)
  )
    return t("PRICING.VALIDATION.QTY_UNIQUE");
  if (!samePricelist.some(isSingleItemPrice)) return t("PRICING.VALIDATION.SINGLE_ITEM");
  return "";
}

/** "min – max" of the list and sale prices, each formatted by `fmt` ("" when there are none). */
export function priceRange(prices: Price[], fmt: (v: number) => string) {
  const all = prices.flatMap((p) => [p.list, p.sale]).filter((x): x is number => typeof x === "number");
  if (!all.length) return "";
  const [min, max] = [Math.min(...all), Math.max(...all)];
  return min === max ? fmt(min) : `${fmt(min)} – ${fmt(max)}`;
}

/** A node of the assignment's condition tree; leaf fields depend on the condition type (`id`). */
export interface Condition {
  id?: string;
  all?: boolean;
  not?: boolean;
  value?: string | number | null;
  secondValue?: string | number | null;
  compareCondition?: string;
  matchCondition?: string;
  group?: string | null;
  children?: Condition[];
  availableChildren?: Condition[];
}

// --- Export module integration (legacy: export toolbar commands when virtoCommerce.exportModule is loaded) ---

/**
 * True when the Export module's UI is loaded (its "ExportSettings" blade is registered) and the user may use it
 * (export:access, which its API requires). Call from setup: the module registry is filled as the remotes load.
 */
export function useExportAvailable() {
  const modulesReady = inject(ModulesReadyKey, ref(true));
  const { hasAccess } = usePermissions();
  return computed(() => modulesReady.value && !!getBladeConfig("ExportSettings") && hasAccess("export:access"));
}

/** Options for the Export module's "ExportSettings" blade, pre-filled with a type and a data query (title/subtitle: i18n keys). */
export function exportOptions(
  exportTypeName: string,
  dataQuery: Record<string, unknown>,
  totalItemsCount: number | undefined,
  title: string,
  subtitle: string,
) {
  return { exportDataRequest: { exportTypeName, dataQuery }, totalItemsCount, title, subtitle };
}
