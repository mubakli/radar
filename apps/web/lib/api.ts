export type StorySummary = { id: string; title: string; summary: string; createdAt: string };
export type StorySourceItem = { id: string; title: string; canonicalLocator: string; observedAt: string; membershipMethod: string; membershipReason: string; source: { id: string; name: string; locator: string } };
export type StoryDetail = StorySummary & { sourceItems: StorySourceItem[] };
export type FetchResult = { attemptedAt: string; succeeded: boolean; entryCount: number; insertedCount: number; skippedCount: number; failureCategory?: string; message?: string };
export type Source = { id: string; name: string; locator: string; enabled: boolean; createdAt: string; lastFetch?: FetchResult };
export type Feedback = { read: boolean; important: boolean; saved: boolean; notRelevant: boolean };
export type BriefItem = { id: string; title: string; locator: string; publishedAt?: string; observedAt: string; sourceId: string; sourceName: string; sourcePriority: number; reason: string; feedback: Feedback };
export type Brief = { date: string; timezone: string; limit: number; count: number; completed: boolean; items: BriefItem[] };
export type SourceItem = { id: string; title: string; url?: string; publishedAt?: string; author?: string; summary?: string; observedAt: string; canonicalLocator: string };

const apiBaseUrl = process.env.RADAR_API_URL ?? "http://localhost:5000";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, { cache: "no-store", ...init });
  if (!response.ok) throw new Error(response.status === 404 ? "not-found" : "api-error");
  return response.json() as Promise<T>;
}

export const getStories = () => request<StorySummary[]>("/api/stories");
export const getStory = (id: string) => request<StoryDetail>(`/api/stories/${id}`);
export const getSources = () => request<Source[]>("/api/sources");
export const getSourceItems = (id: string) => request<SourceItem[]>(`/api/sources/${id}/items`);
export const createSource = (name: string, locator: string) => request<Source>("/api/sources", { method: "POST", body: JSON.stringify({ name, locator }), headers: { "Content-Type": "application/json" } });
export const fetchSource = (id: string) => request<FetchResult>(`/api/sources/${id}/fetch`, { method: "POST" });
export const setSourceEnabled = (id: string, enabled: boolean) => request<{ id: string; enabled: boolean }>(`/api/sources/${id}/enabled`, { method: "PATCH", body: JSON.stringify({ enabled }), headers: { "Content-Type": "application/json" } });
export const getBrief = (date?: string, timezone = "UTC") => request<Brief>(`/api/brief?timezone=${encodeURIComponent(timezone)}${date ? `&date=${date}` : ""}`);
export const setFeedback = (id: string, action: "read" | "important" | "saved" | "not relevant", value: boolean) => request<Feedback>(`/api/brief/items/${id}/feedback`, { method: "PUT", body: JSON.stringify({ action, value }), headers: { "Content-Type": "application/json" } });
