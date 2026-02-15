import Link from 'next/link';

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-24">
      <h1 className="text-4xl font-bold mb-8">FleetFuel</h1>
      <p className="text-xl mb-8 text-center max-w-md">
        Track your vehicle trips and fuel receipts. Generate tax-ready CSV summaries.
      </p>
      <div className="flex gap-4">
        <Link 
          href="/login"
          className="px-6 py-3 bg-primary text-primary-foreground rounded-lg hover:opacity-90"
        >
          Login
        </Link>
        <Link 
          href="/register"
          className="px-6 py-3 border border-input bg-background hover:bg-accent rounded-lg"
        >
          Register
        </Link>
      </div>
    </main>
  );
}
