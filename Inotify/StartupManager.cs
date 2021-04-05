using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Inotify
{
    public class StartUpManager
    {
        private static StartUpManager _appManager;
        private CancellationTokenSource _tokenSource;
        private bool _running;
        private bool _restart;

        public bool Restarting => _restart;

        public StartUpManager()
        {
            _running = false;
            _restart = false;

        }

        public static StartUpManager Load()
        {
            if (_appManager == null)
            {
                _appManager = new StartUpManager();
            }

            return _appManager;
        }

        public void Start(string[] args)
        {
            if (_running)
            {
                return;
            }

            if (_tokenSource != null && _tokenSource.IsCancellationRequested)
            {
                return;
            }

            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.ThrowIfCancellationRequested();
            _running = true;

            var hostBuilder = Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>();
             }).Build();
            hostBuilder.RunAsync(_tokenSource.Token);
            hostBuilder.WaitForShutdown();
        }

        public void Stop()
        {
            if (!_running)
            {
                return;
            }

            _tokenSource.Cancel();
            _running = false;
        }

        public void Restart()
        {
            Stop();

            _restart = true;
            _tokenSource = null;
        }
    }
}
