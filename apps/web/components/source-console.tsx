"use client";

import { useState } from "react";
import type { FormEvent } from "react";
import { createSource, fetchSource, getSourceItems, setSourceEnabled, type Source, type SourceItem } from "../lib/api";

export default function SourceConsole({ initialSources }: { initialSources: Source[] }) {
  const [sources, setSources] = useState(initialSources);
  const [items, setItems] = useState<Record<string, SourceItem[]>>({});
  const [name, setName] = useState("");
  const [locator, setLocator] = useState("");
  const [message, setMessage] = useState("");

  async function add(event: FormEvent) {
    event.preventDefault();
    try {
      const source = await createSource(name, locator);
      setSources(current => [...current, source]);
      setName("");
      setLocator("");
      setMessage("Source added.");
    } catch {
      setMessage("Source could not be added.");
    }
  }

  async function fetchNow(source: Source) {
    try {
      const result = await fetchSource(source.id);
      setSources(current => current.map(item => item.id === source.id ? { ...item, lastFetch: result } : item));
      const fetched = result.succeeded ? await getSourceItems(source.id) : items[source.id] ?? [];
      setItems(current => ({ ...current, [source.id]: fetched }));
      setMessage(result.succeeded ? `Fetched ${result.insertedCount} new Source Items.` : result.message ?? "Fetch failed.");
    } catch {
      setMessage("Fetch failed.");
    }
  }

  async function toggle(source: Source) {
    try {
      const result = await setSourceEnabled(source.id, !source.enabled);
      setSources(current => current.map(item => item.id === source.id ? { ...item, enabled: result.enabled } : item));
      setMessage(result.enabled ? "Source enabled." : "Source disabled.");
    } catch {
      setMessage("Source state could not be changed.");
    }
  }

  function isFetchable(source: Source) {
    return /^https?:\/\//i.test(source.locator);
  }

  return <section className="mb-14">
    <p className="mb-3 text-sm font-semibold uppercase tracking-[0.3em] text-cyan-400">Radar / Sources</p>
    <h1 className="mb-6 text-4xl font-bold tracking-tight">Collect real signals.</h1>
    <form onSubmit={add} className="mb-6 grid gap-3 rounded-xl border border-slate-800 bg-slate-900/60 p-5 md:grid-cols-[1fr_2fr_auto]">
      <input required aria-label="Source name" value={name} onChange={event => setName(event.target.value)} placeholder="Source name" className="rounded border border-slate-700 bg-slate-950 p-3" />
      <input required type="url" aria-label="Feed URL" value={locator} onChange={event => setLocator(event.target.value)} placeholder="https://example.com/feed.xml" className="rounded border border-slate-700 bg-slate-950 p-3" />
      <button className="rounded bg-cyan-500 px-4 py-2 font-semibold text-slate-950">Add Source</button>
    </form>
    {message && <p role="status" className="mb-4 text-sm text-cyan-300">{message}</p>}
    <div className="grid gap-4">{sources.map(source => <article key={source.id} className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
      <div className="flex flex-wrap items-center justify-between gap-3"><div><h2 className="font-semibold">{source.name}</h2><p className="break-all text-sm text-slate-500">{source.locator}</p><p className="text-sm">{source.enabled ? "Active" : "Disabled"}{!isFetchable(source) && " · Internal source"}</p></div><div className="flex gap-2"><button onClick={() => toggle(source)} className="rounded border border-slate-700 px-3 py-2 text-sm">{source.enabled ? "Disable" : "Enable"}</button>{isFetchable(source) && <button disabled={!source.enabled} onClick={() => fetchNow(source)} className="rounded bg-cyan-500 px-3 py-2 text-sm font-semibold text-slate-950 disabled:cursor-not-allowed disabled:opacity-40">Fetch now</button>}</div></div>
      {source.lastFetch && <p className="mt-3 text-sm text-slate-400">{source.lastFetch.succeeded ? `Success: ${source.lastFetch.insertedCount} new, ${source.lastFetch.entryCount} entries` : `Failure: ${source.lastFetch.message}`}</p>}
      {items[source.id]?.map(item => <div key={item.id} className="mt-4 border-t border-slate-800 pt-4"><h3 className="font-medium">{item.title}</h3>{item.summary && <p className="mt-1 text-sm text-slate-400">{item.summary}</p>}<a target="_blank" rel="noopener noreferrer" href={item.url ?? undefined} className="mt-2 block break-all text-sm text-cyan-400">{item.url ?? item.canonicalLocator}</a></div>)}
    </article>)}</div>
  </section>;
}
