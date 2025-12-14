using Microsoft.Extensions.Logging;
using IT13_Final.Services.Auth;
using IT13_Final.Services.Audit;
using IT13_Final.Services.Data;

namespace IT13_Final
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<AuthState>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPhilippineAddressService, PhilippineAddressService>();
            builder.Services.AddScoped<IColorService, ColorService>();
            builder.Services.AddScoped<ISizeService, SizeService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IVariantService, VariantService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IStockInService, StockInService>();
            builder.Services.AddScoped<IStockOutService, StockOutService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
            builder.Services.AddScoped<ISalesService, SalesService>();
            builder.Services.AddScoped<IReturnsService, ReturnsService>();
            builder.Services.AddScoped<IDailySalesVerificationService, DailySalesVerificationService>();
            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<ISupplierInvoiceService, SupplierInvoiceService>();
            builder.Services.AddScoped<IIncomeBreakdownService, IncomeBreakdownService>();
            builder.Services.AddScoped<ICashflowAuditService, CashflowAuditService>();
            builder.Services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // Auto-sync background service (syncs local to Azure every hour)
            builder.Services.AddSingleton<AutoSyncBackgroundService>(sp =>
            {
                var syncService = new DatabaseSyncService();
                var autoSync = new AutoSyncBackgroundService(syncService);
                autoSync.Start(); // Start automatic sync on app launch
                return autoSync;
            });

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
