import { computed, ref, type Ref } from "vue";

export interface ListFilter {
  id: string;
  label: string;
  /** value → label */
  options: Ref<Record<string, string>>;
}

interface Options {
  filters: ListFilter[];
  pagination: { skip: number; setPage: (page: number) => void };
  load: () => unknown;
}

/**
 * Multi-select filters for a list blade's search bar: the `global-filters` config, the selected
 * values by filter id (undefined when nothing is picked) and the `@filter` handler, which goes
 * back to page 1 (the paging watcher reloads) or reloads at once.
 */
export function useListFilters({ filters, pagination, load }: Options) {
  const values = ref<Record<string, string[] | undefined>>({});

  const globalFilters = computed(() =>
    filters.map((f) => ({
      id: f.id,
      label: f.label,
      filter: {
        multiple: true,
        options: Object.entries(f.options.value)
          .map(([value, label]) => ({ value, label }))
          .sort((a, b) => a.label.localeCompare(b.label)),
      },
    })),
  );

  function onFilter(event: { filters: Record<string, unknown> }) {
    values.value = Object.fromEntries(
      filters.map((f) => {
        const picked = ([] as string[]).concat((event.filters[f.id] as string | string[] | undefined) ?? []);
        return [f.id, picked.length ? picked : undefined];
      }),
    );
    if (pagination.skip) pagination.setPage(1);
    else load();
  }

  return { values, globalFilters, onFilter };
}
