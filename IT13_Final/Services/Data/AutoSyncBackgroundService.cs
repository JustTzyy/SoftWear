using System.Timers;
using Timer = System.Timers.Timer;

namespace IT13_Final.Services.Data
{
    /// <summary>
    /// Background service that automatically syncs local database to Azure every hour
    /// </summary>
    public class AutoSyncBackgroundService : IDisposable
    {
        private readonly IDatabaseSyncService _syncService;
        private Timer? _timer;
        private bool _isRunning = false;
        private DateTime _lastSyncTime = DateTime.MinValue;
        private string _lastSyncStatus = "Not started";
        private bool _disposed = false;

        // Sync interval in milliseconds (1 hour = 3,600,000 ms)
        private const int SYNC_INTERVAL_MS = 60 * 60 * 1000; // 1 hour
        
        // For testing, you can use shorter intervals:
        // private const int SYNC_INTERVAL_MS = 5 * 60 * 1000; // 5 minutes
        // private const int SYNC_INTERVAL_MS = 60 * 1000; // 1 minute

        public bool IsEnabled { get; private set; } = true;
        public DateTime LastSyncTime => _lastSyncTime;
        public string LastSyncStatus => _lastSyncStatus;
        public bool IsRunning => _isRunning;
        public int SyncIntervalMinutes => SYNC_INTERVAL_MS / 60000;

        public event EventHandler<SyncCompletedEventArgs>? SyncCompleted;

        public AutoSyncBackgroundService(IDatabaseSyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>
        /// Starts the automatic sync timer
        /// </summary>
        public void Start()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            _timer = new Timer(SYNC_INTERVAL_MS);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
            IsEnabled = true;

            Console.WriteLine($"[AutoSync] Background sync service started. Syncing every {SyncIntervalMinutes} minutes.");
            
            // Optionally run an initial sync after a short delay (30 seconds)
            _ = Task.Delay(30000).ContinueWith(_ => TriggerSyncAsync());
        }

        /// <summary>
        /// Stops the automatic sync timer
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            IsEnabled = false;
            Console.WriteLine("[AutoSync] Background sync service stopped.");
        }

        /// <summary>
        /// Manually triggers a sync operation
        /// </summary>
        public async Task<SyncResult> TriggerSyncAsync()
        {
            if (_isRunning)
            {
                return new SyncResult
                {
                    Success = false,
                    Message = "A sync operation is already in progress."
                };
            }

            return await PerformSyncAsync();
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsEnabled || _isRunning)
                return;

            await PerformSyncAsync();
        }

        private async Task<SyncResult> PerformSyncAsync()
        {
            if (_isRunning)
            {
                return new SyncResult { Success = false, Message = "Sync already in progress" };
            }

            _isRunning = true;
            _lastSyncStatus = "Syncing...";
            
            Console.WriteLine($"[AutoSync] Starting automatic sync at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            SyncResult result;
            try
            {
                // Test connections first
                var localConnected = await _syncService.TestLocalConnectionAsync();
                var azureConnected = await _syncService.TestAzureConnectionAsync();

                if (!localConnected)
                {
                    result = new SyncResult
                    {
                        Success = false,
                        Message = "Local database connection failed. Sync skipped."
                    };
                    _lastSyncStatus = "Failed: Local DB not connected";
                    Console.WriteLine("[AutoSync] " + result.Message);
                }
                else if (!azureConnected)
                {
                    result = new SyncResult
                    {
                        Success = false,
                        Message = "Azure database connection failed. Sync skipped."
                    };
                    _lastSyncStatus = "Failed: Azure DB not connected";
                    Console.WriteLine("[AutoSync] " + result.Message);
                }
                else
                {
                    // Perform the sync (Local → Azure)
                    result = await _syncService.SyncAllTablesAsync();
                    
                    _lastSyncTime = DateTime.Now;
                    _lastSyncStatus = result.Success 
                        ? $"Success: {result.RowsSynced} rows synced" 
                        : $"Completed with errors: {result.Message}";
                    
                    Console.WriteLine($"[AutoSync] Sync completed: {result.Message}");
                    Console.WriteLine($"[AutoSync] Rows synced: {result.RowsSynced}, Rows skipped: {result.RowsSkipped}");
                }
            }
            catch (Exception ex)
            {
                result = new SyncResult
                {
                    Success = false,
                    Message = $"Error during sync: {ex.Message}"
                };
                _lastSyncStatus = $"Error: {ex.Message}";
                Console.WriteLine($"[AutoSync] Error: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }

            // Raise event for any listeners
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs(result, _lastSyncTime));

            return result;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _disposed = true;
            }
        }
    }

    public class SyncCompletedEventArgs : EventArgs
    {
        public SyncResult Result { get; }
        public DateTime SyncTime { get; }

        public SyncCompletedEventArgs(SyncResult result, DateTime syncTime)
        {
            Result = result;
            SyncTime = syncTime;
        }
    }
}


