import { expect, test } from "@playwright/test";

test("user can complete a daily brief item and keep feedback after reload", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByText("Today's technical signal")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Radar Development Fixture" })).toBeVisible();
  const fixtureCard = page.getByRole("article").filter({ has: page.getByRole("link", { name: "PostgreSQL 18 improves query execution" }) });
  await expect(fixtureCard.getByText(/Source priority/)).toBeVisible();
  await fixtureCard.getByRole("link", { name: "PostgreSQL 18 improves query execution" }).click();
  await expect(page.getByText("Source provenance")).toBeVisible();
  await expect(page.getByText(/-v1/)).toBeVisible();
  await expect(page.getByRole("button", { name: "Split into its own Story" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Merge" })).toBeVisible();
  await page.getByRole("link", { name: "Daily brief" }).click();
  const unreadButton = fixtureCard.getByRole("button", { name: "Read", exact: true });
  if (await unreadButton.isVisible()) await unreadButton.click();
  await expect(fixtureCard.getByRole("button", { name: "✓ Read" })).toBeVisible();
  await page.reload();
  await expect(fixtureCard.getByRole("button", { name: "✓ Read" })).toBeVisible();
});
