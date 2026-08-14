import { expect, test } from "@playwright/test";

test("user can complete a daily brief item and keep feedback after reload", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByText("Today's technical signal")).toBeVisible();
  await expect(page.getByText("Radar Development Fixture")).toBeVisible();
  await expect(page.getByText(/Source priority/)).toBeVisible();
  await page.getByRole("button", { name: "Read" }).click();
  await expect(page.getByRole("button", { name: "✓ Read" })).toBeVisible();
  await page.reload();
  await expect(page.getByRole("button", { name: "✓ Read" })).toBeVisible();
});
