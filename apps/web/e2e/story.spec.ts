import { expect, test } from "@playwright/test";

test("user can complete a daily brief item and keep feedback after reload", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByText("Today's technical signal")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Radar Development Fixture" })).toBeVisible();
  await expect(page.getByText(/Source priority/)).toBeVisible();
  await page.getByRole("link", { name: "PostgreSQL 18 improves query execution" }).click();
  await expect(page.getByText("Source provenance")).toBeVisible();
  await expect(page.getByText(/fixture-v1/)).toBeVisible();
  await expect(page.getByRole("button", { name: "Split into its own Story" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Merge" })).toBeVisible();
  await page.getByRole("link", { name: "Daily brief" }).click();
  await page.getByRole("button", { name: "Read" }).click();
  await expect(page.getByRole("button", { name: "✓ Read" })).toBeVisible();
  await page.reload();
  await expect(page.getByRole("button", { name: "✓ Read" })).toBeVisible();
});
