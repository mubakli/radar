import "./globals.css";

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="en"><body><main className="mx-auto min-h-screen max-w-5xl px-6 py-10">{children}</main></body></html>;
}
