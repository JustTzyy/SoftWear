namespace IT13_Final.Services.Auth
{
    public sealed class AuthUser
    {
        public int Id { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string Role { get; init; }
    }

    public sealed class AuthState
    {
        public AuthUser? CurrentUser { get; private set; }

        public event Action? Changed;

        public void SetUser(AuthUser user)
        {
            CurrentUser = user;
            Changed?.Invoke();
        }

        public void Logout()
        {
            CurrentUser = null;
            Changed?.Invoke();
        }
    }
}


