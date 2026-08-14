"use client";

import { useState } from "react";
import Link from "next/link";
import { setFeedback, type BriefStory, type Feedback } from "../lib/api";

export default function BriefCard({ story }: { story: BriefStory }) {
  const [feedback, setLocalFeedback] = useState<Feedback>(story.feedback);
  async function update(action: "read" | "important" | "saved" | "not relevant") {
    const value = !(action === "read" ? feedback.read : action === "important" ? feedback.important : action === "saved" ? feedback.saved : feedback.notRelevant);
    const next = await setFeedback(story.feedbackSourceItemId, action, value);
    setLocalFeedback(next);
  }
  return <article className="rounded-xl border border-slate-800 bg-slate-900/60 p-6"><h2 className="text-xl font-semibold"><Link href={story.locator} className="hover:text-cyan-300">{story.title}</Link></h2><p className="mt-3 text-sm text-slate-400">{story.sources.map(source => source.name).join(" · ")} · {story.itemCount} item{story.itemCount === 1 ? "" : "s"} · {story.reason}</p><p className="mt-2 text-xs text-slate-500">{story.publishedAt ? formatDate(story.publishedAt) : `Observed ${formatDate(story.observedAt)}`}</p><div className="mt-5 flex flex-wrap gap-2">{([['read', 'Read'], ['important', 'Important'], ['saved', 'Saved'], ['not relevant', 'Not relevant']] as const).map(([action, label]) => <button key={action} onClick={() => update(action)} className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-300 hover:border-cyan-400">{(action === "read" ? feedback.read : action === "important" ? feedback.important : action === "saved" ? feedback.saved : feedback.notRelevant) ? "✓ " : ""}{label}</button>)}</div></article>;
}

function formatDate(value: string) { return new Date(value).toISOString().replace("T", " ").replace(".000Z", " UTC"); }
