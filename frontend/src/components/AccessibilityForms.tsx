import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export function LoginForm() {
  return (
    <form className="space-y-4" aria-label="Login form">
      <div className="space-y-2">
        <Label htmlFor="email">Email</Label>
        <Input
          id="email"
          type="email"
          placeholder="name@example.com"
          autoComplete="email"
          required
          aria-required="true"
          aria-describedby="email-hint"
        />
        <p id="email-hint" className="text-sm text-muted-foreground">
          Enter your email address
        </p>
      </div>
      
      <div className="space-y-2">
        <Label htmlFor="password">Password</Label>
        <Input
          id="password"
          type="password"
          placeholder="••••••••"
          autoComplete="current-password"
          required
          aria-required="true"
          aria-describedby="password-hint"
        />
        <p id="password-hint" className="text-sm text-muted-foreground">
          Enter your password
        </p>
      </div>
      
      <Button type="submit" className="w-full">
        Sign In
      </Button>
    </form>
  );
}

export function RegisterForm() {
  return (
    <form className="space-y-4" aria-label="Registration form">
      <div className="space-y-2">
        <Label htmlFor="name">Full Name</Label>
        <Input
          id="name"
          type="text"
          placeholder="John Doe"
          autoComplete="name"
          required
          aria-required="true"
        />
      </div>
      
      <div className="space-y-2">
        <Label htmlFor="reg-email">Email</Label>
        <Input
          id="reg-email"
          type="email"
          placeholder="name@example.com"
          autoComplete="email"
          required
          aria-required="true"
        />
      </div>
      
      <div className="space-y-2">
        <Label htmlFor="reg-password">Password</Label>
        <Input
          id="reg-password"
          type="password"
          required
          aria-required="true"
          aria-describedby="password-requirements"
        />
        <p id="password-requirements" className="text-sm text-muted-foreground">
          Must be at least 8 characters
        </p>
      </div>
      
      <Button type="submit" className="w-full">
        Create Account
      </Button>
    </form>
  );
}