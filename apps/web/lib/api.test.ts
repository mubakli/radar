import { describe, expect, it, vi } from "vitest";
import { getBrief, getStories, setFeedback } from "./api";

describe("story API client", () => {
  it("returns the story list from the configured API", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => [{ id: "1", title: "Seed Story" }] }));
    await expect(getStories()).resolves.toEqual([{ id: "1", title: "Seed Story" }]);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/stories", { cache: "no-store" });
    vi.unstubAllGlobals();
  });
});

describe("getBrief", () => {
  it("requests brief with default UTC timezone when no arguments provided", async () => {
    const brief = { date: "2026-08-14", timezone: "UTC", limit: 20, count: 0, completed: true, stories: [] };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => brief }));
    await expect(getBrief()).resolves.toEqual(brief);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/brief?timezone=UTC", { cache: "no-store" });
    vi.unstubAllGlobals();
  });

  it("encodes custom timezone and includes date when provided", async () => {
    const brief = { date: "2026-08-14", timezone: "Europe/Istanbul", limit: 5, count: 1, completed: false, stories: [] };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => brief }));
    await expect(getBrief("2026-08-14", "Europe/Istanbul")).resolves.toEqual(brief);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/brief?timezone=Europe%2FIstanbul&date=2026-08-14", { cache: "no-store" });
    vi.unstubAllGlobals();
  });

  it("throws on non-ok response", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status: 500 }));
    await expect(getBrief()).rejects.toThrow("api-error");
    vi.unstubAllGlobals();
  });
});

describe("setFeedback", () => {
  it("sends PUT with correct action and value", async () => {
    const feedback = { read: true, important: false, saved: false, notRelevant: false };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => feedback }));
    await expect(setFeedback("item-1", "read", true)).resolves.toEqual(feedback);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/brief/items/item-1/feedback", {
      cache: "no-store",
      method: "PUT",
      body: JSON.stringify({ action: "read", value: true }),
      headers: { "Content-Type": "application/json" },
    });
    vi.unstubAllGlobals();
  });

  it("encodes 'not relevant' action correctly", async () => {
    const feedback = { read: false, important: false, saved: false, notRelevant: true };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => feedback }));
    await expect(setFeedback("item-2", "not relevant", true)).resolves.toEqual(feedback);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/brief/items/item-2/feedback", {
      cache: "no-store",
      method: "PUT",
      body: JSON.stringify({ action: "not relevant", value: true }),
      headers: { "Content-Type": "application/json" },
    });
    vi.unstubAllGlobals();
  });

  it("throws not-found for 404 response", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status: 404 }));
    await expect(setFeedback("missing", "read", true)).rejects.toThrow("not-found");
    vi.unstubAllGlobals();
  });
});
