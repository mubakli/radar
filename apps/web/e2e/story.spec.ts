import { expect, test } from "@playwright/test";

test("user can open the seeded story and inspect provenance", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByText("PostgreSQL 18 improves query execution").first()).toBeVisible();
  await page.getByText("PostgreSQL 18 improves query execution").first().click();
  await expect(page.getByText("Source provenance")).toBeVisible();
  await expect(page.getByText("Radar Development Fixture")).toBeVisible();
  await expect(page.getByText("https://example.com/radar/postgresql-18-query-execution")).toBeVisible();
});
