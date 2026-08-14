import { getBrief, getSources, type Brief, type Source } from "../lib/api";
import SourceConsole from "../components/source-console";
import BriefCard from "../components/brief-card";

export default async function BriefPage() {
  const brief = await loadBrief();
  if (!brief) return <ErrorMessage />;
  return <><SourceConsole initialSources={await loadSources()} /><p className="mb-3 text-sm font-semibold uppercase tracking-[0.3em] text-cyan-400">Radar / Daily brief</p><h1 className="text-4xl font-bold tracking-tight">Today&apos;s technical signal</h1><p className="mt-3 text-slate-400">{brief.count} of {brief.limit} Stories. {brief.completed ? "Brief complete." : "Finish by marking each Story read or not relevant."}</p>{brief.stories.length === 0 ? <p className="mt-10 rounded-xl border border-slate-800 p-6 text-slate-400">No Stories for today.</p> : <div className="mt-10 grid gap-4">{brief.stories.map(story => <BriefCard key={story.id} story={story} />)}</div>}</>;
}

async function loadBrief(): Promise<Brief | null> { try { return await getBrief(); } catch { return null; } }
async function loadSources(): Promise<Source[]> { try { return await getSources(); } catch { return []; } }
function ErrorMessage() { return <><h1 className="text-3xl font-bold">Radar is unavailable</h1><p className="mt-3 text-slate-400">Start the API and database, then reload this page.</p></>; }
