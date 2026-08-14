import Link from "next/link";
import { getStories, type StorySummary } from "../lib/api";
import { getSources, type Source } from "../lib/api";
import SourceConsole from "../components/source-console";

export default async function StoriesPage() {
  const result = await loadStories();
  if (!result.ok) return <ErrorMessage />;
  const sources = await loadSources();
  return <><SourceConsole initialSources={sources} /><StoryList stories={result.stories} /></>;
}

async function loadStories(): Promise<{ ok: true; stories: StorySummary[] } | { ok: false }> {
  try { return { ok: true, stories: await getStories() }; } catch { return { ok: false }; }
}

function StoryList({ stories }: { stories: StorySummary[] }) {
  return <><p className="mb-3 text-sm font-semibold uppercase tracking-[0.3em] text-cyan-400">Radar / Stories</p><h1 className="mb-10 text-4xl font-bold tracking-tight">Technical signal, preserved.</h1>{stories.length === 0 ? <p className="rounded-xl border border-slate-800 p-6 text-slate-400">No stories yet.</p> : <div className="grid gap-4">{stories.map(story => <Link key={story.id} href={`/stories/${story.id}`} className="rounded-xl border border-slate-800 bg-slate-900/60 p-6 hover:border-cyan-400"><h2 className="text-xl font-semibold">{story.title}</h2><p className="mt-2 text-slate-400">{story.summary}</p><time className="mt-5 block text-sm text-slate-500">{new Date(story.createdAt).toLocaleDateString()}</time></Link>)}</div>}</>;
}

function ErrorMessage() { return <><h1 className="text-3xl font-bold">Radar is unavailable</h1><p className="mt-3 text-slate-400">Start the API and database, then reload this page.</p></>; }

async function loadSources(): Promise<Source[]> { try { return await getSources(); } catch { return []; } }
