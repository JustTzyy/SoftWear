namespace IT13_Final.Services.Auth
{
    public sealed class AuthUser
    {
        public int Id { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string Role { get; init; }
    }

    /// <summary>
    /// Represents the current user's subscription information
    /// </summary>
    public sealed class SubscriptionInfo
    {
        public int SubscriptionId { get; init; }
        public int PlanId { get; init; }
        public required string PlanName { get; init; }
        public required string PlanCode { get; init; }
        public decimal AdminFeePercentage { get; init; }
        public bool HasStockClerkAccess { get; init; }
        public bool HasCashierAccess { get; init; }
        public bool HasAccountingAccess { get; init; }
        public bool HasFullReportsAccess { get; init; }
        public DateTime StartDate { get; init; }
        public required string Status { get; init; }
    }

    public sealed class AuthState
    {
        public AuthUser? CurrentUser { get; private set; }
        public SubscriptionInfo? CurrentSubscription { get; private set; }

        public event Action? Changed;

        public void SetUser(AuthUser user)
        {
            CurrentUser = user;
            Changed?.Invoke();
        }

        public void SetSubscription(SubscriptionInfo? subscription)
        {
            CurrentSubscription = subscription;
            Changed?.Invoke();
        }

        public void SetUserWithSubscription(AuthUser user, SubscriptionInfo? subscription)
        {
            CurrentUser = user;
            CurrentSubscription = subscription;
            Changed?.Invoke();
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentSubscription = null;
            Changed?.Invoke();
        }

        /// <summary>
        /// Check if the current user has access to a specific module based on their subscription
        /// </summary>
        public bool CanAccessModule(string moduleName)
        {
            // Non-sellers don't need subscription validation
            if (CurrentUser?.Role?.ToLowerInvariant() != "seller")
            {
                return true;
            }

            // Sellers need active subscription
            if (CurrentSubscription == null)
            {
                return false;
            }

            return moduleName.ToLowerInvariant() switch
            {
                "stockclerk" or "stock_clerk" or "inventory" => CurrentSubscription.HasStockClerkAccess,
                "cashier" or "pos" or "sales" => CurrentSubscription.HasCashierAccess,
                "accounting" or "finance" => CurrentSubscription.HasAccountingAccess,
                "reports" or "analytics" => CurrentSubscription.HasFullReportsAccess,
                _ => false
            };
        }

        /// <summary>
        /// Check if the current seller has an active subscription
        /// </summary>
        public bool HasActiveSubscription => CurrentSubscription != null && CurrentSubscription.Status == "Active";

        /// <summary>
        /// Get the admin fee percentage for the current subscription
        /// </summary>
        public decimal AdminFeePercentage => CurrentSubscription?.AdminFeePercentage ?? 0m;
    }
}


