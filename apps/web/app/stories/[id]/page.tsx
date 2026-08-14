import Link from "next/link";
import { getStory, type StoryDetail } from "../../../lib/api";

export default async function StoryPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const result = await loadStory(id);
  if (result.kind !== "ok") return result.kind === "not-found" ? <NotFound /> : <ErrorMessage />;
  return <StoryContent story={result.story} />;
}

async function loadStory(id: string): Promise<{ kind: "ok"; story: StoryDetail } | { kind: "not-found" | "error" }> {
  try { return { kind: "ok", story: await getStory(id) }; } catch (error) { return { kind: error instanceof Error && error.message === "not-found" ? "not-found" : "error" }; }
}

function StoryContent({ story }: { story: StoryDetail }) {
  return <><Link href="/" className="text-sm text-cyan-400 hover:text-cyan-300">← All stories</Link><p className="mt-10 text-sm font-semibold uppercase tracking-[0.3em] text-cyan-400">Story detail</p><h1 className="mt-3 text-4xl font-bold tracking-tight">{story.title}</h1><p className="mt-4 max-w-2xl text-lg text-slate-300">{story.summary}</p><section className="mt-12"><h2 className="text-sm font-semibold uppercase tracking-widest text-slate-500">Source provenance</h2><div className="mt-4 grid gap-4">{story.sourceItems.map(item => <article key={item.id} className="rounded-xl border border-slate-800 bg-slate-900/60 p-6"><h3 className="text-lg font-semibold">{item.title}</h3><dl className="mt-5 grid gap-3 text-sm"><div><dt className="text-slate-500">Source</dt><dd>{item.source.name} <span className="text-slate-500">({item.source.locator})</span></dd></div><div><dt className="text-slate-500">Canonical locator</dt><dd><a className="break-all text-cyan-400 hover:text-cyan-300" href={item.canonicalLocator}>{item.canonicalLocator}</a></dd></div><div><dt className="text-slate-500">Observed</dt><dd>{new Date(item.observedAt).toLocaleString()}</dd></div><div><dt className="text-slate-500">Story membership</dt><dd>{item.membershipMethod}: {item.membershipReason}</dd></div></dl></article>)}</div></section></>;
}

function NotFound() { return <><h1 className="text-3xl font-bold">Story not found</h1><Link href="/" className="mt-4 inline-block text-cyan-400">Return to stories</Link></>; }
function ErrorMessage() { return <><h1 className="text-3xl font-bold">Radar is unavailable</h1><p className="mt-3 text-slate-400">Start the API and database, then reload this page.</p></>; }
