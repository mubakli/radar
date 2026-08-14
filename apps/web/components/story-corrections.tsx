"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { mergeStories, splitStoryItem } from "../lib/api";

export function MergeStoryControl({ storyId }: { storyId: string }) {
  const router = useRouter();
  const [sourceId, setSourceId] = useState("");
  const [reason, setReason] = useState("These Stories describe the same development.");
  return <div className="mt-6 grid gap-3 rounded-xl border border-slate-800 p-4 sm:grid-cols-[1fr_1fr_auto]"><input aria-label="Story ID to merge" value={sourceId} onChange={event => setSourceId(event.target.value)} placeholder="Story ID to merge" className="rounded-lg bg-slate-950 px-3 py-2"/><input aria-label="Merge reason" value={reason} onChange={event => setReason(event.target.value)} className="rounded-lg bg-slate-950 px-3 py-2"/><button onClick={async () => { await mergeStories(storyId, sourceId, reason); router.refresh(); }} className="rounded-lg border border-cyan-500 px-4 py-2 text-cyan-300">Merge</button></div>;
}

export function SplitItemControl({ storyId, sourceItemId }: { storyId: string; sourceItemId: string }) {
  const router = useRouter();
  return <button onClick={async () => { const result = await splitStoryItem(storyId, sourceItemId, "This item describes a different development."); router.push(`/stories/${result.storyId}`); }} className="mt-4 rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-300 hover:border-cyan-400">Split into its own Story</button>;
}
