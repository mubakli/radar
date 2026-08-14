import { describe, expect, it, vi } from "vitest";
import { getStories } from "./api";

describe("story API client", () => {
  it("returns the story list from the configured API", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true, json: async () => [{ id: "1", title: "Seed Story" }] }));
    await expect(getStories()).resolves.toEqual([{ id: "1", title: "Seed Story" }]);
    expect(fetch).toHaveBeenCalledWith("http://localhost:5000/api/stories", { cache: "no-store" });
    vi.unstubAllGlobals();
  });
});
