using GabsHybridApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;

namespace GabsHybridApp.Shared.Layout
{
    public partial class NotificationPanel
    {
        [Parameter] public string? NotificationHubUrl { get; set; } // https://host/notificationhub
        [Parameter] public bool IncludeSample { get; set; } = false;

        [CascadingParameter] public Task<AuthenticationState>? AuthState { get; set; }
        private Guid? _userId;
        private List<Notification> _notifications = new();
        private HubConnection? _hubConnection;
        private bool _isConnected = false;
        private bool _hasNewNotification = false;

        protected override async Task OnInitializedAsync()
        {
            if (AuthState is not null)
            {
                var state = await AuthState;
                var userIdClaim = state.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _userId = Guid.Parse(userIdClaim);

                    // Fetch notifications from DB
                    var dbNotifications = await NotificationService.GetUserNotificationsAsync(_userId.Value);
                    _notifications = new List<Notification>(dbNotifications);

                    // Optionally add sample notifications
                    if (IncludeSample)
                        _notifications.AddRange(GetSampleNotifications());

                    await InitializeSignalRAsync();
                }
            }
        }

        private async Task InitializeSignalRAsync()
        {
            string GetNotificationHubUrl()
            {
                if(!string.IsNullOrWhiteSpace(NotificationHubUrl)) return NotificationHubUrl;

                var baseUri = NavManager.BaseUri;

                bool isRunningInDocker =
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

                if (isRunningInDocker && baseUri.Contains("localhost"))
                {
                    return baseUri.Replace("localhost", "host.docker.internal").TrimEnd('/') + "/notificationhub";
                }

                return new Uri(new Uri(baseUri), "notificationhub").ToString();
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(GetNotificationHubUrl())
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<Notification>("ReceiveNotification", async (notification) =>
            {
                if (notification.UserId != _userId)
                {
                    await InvokeAsync(() =>
                    {
                        _notifications.Insert(0, notification);
                        _hasNewNotification = true;
                        StateHasChanged();
                    });
                }
            });

            const int maxRetries = 5;
            int retryCount = 0;

            while (!_isConnected && retryCount < maxRetries)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _isConnected = true;
                    Console.WriteLine($"SignalR Connected on attempt {retryCount + 1}: {_hubConnection.State}");
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"SignalR connection failed: {ex.Message}. Retrying {retryCount}/{maxRetries}...");
                    await Task.Delay(1000);
                }
            }

            if (!_isConnected)
            {
                Console.WriteLine("SignalR connection could not be established after retries. Notifications will be disabled.");
            }
        }

        private List<Notification> GetSampleNotifications()
        {
            var rand = new Random();

            var titles = new[]
            {
                "System Alert", "Reminder", "Weekly Summary", "Account Notice",
                "Task Completed", "Welcome!", "Security Notice", "Promo Update"
            };

            var contents = new[]
            {
                "Don't forget to verify your email address.",
                "Your password was recently changed.\nContact support if this wasn’t you.",
                "Someone accessed your account from a new location.\nPlease review your recent activity.",
                "Terms of Service updated.\nReview the changes before continuing.",
                "You've earned a new badge today! 🎉 Keep it up!",
                "Project deadline is tomorrow.\nMake sure everything is submitted.",
                "Check your weekly report.\nLots of new updates available.",
                "This is a test notification.\nIt's a long one.\nJust to mimic reality."
            };

            var types = Enum.GetValues<NotificationType>();
            var list = new List<Notification>();

            list.Add(new Notification
            {
                Id = -1,
                Title = "New Message",
                Content = "You received a new message 2 minutes ago.",
                CreatedOn = DateTime.Now.AddMinutes(-2),
                NotificationType = NotificationType.Info
            });

            list.Add(new Notification
            {
                Id = -2,
                Title = "Activity Log",
                Content = "You logged in from a different browser about an hour ago.",
                CreatedOn = DateTime.Now.AddHours(-1),
                NotificationType = NotificationType.Warning
            });

            list.Add(new Notification
            {
                Id = -3,
                Title = "Daily Report",
                Content = "Your daily report is now available.\nSee the insights inside.",
                CreatedOn = DateTime.Now.AddDays(-1),
                NotificationType = NotificationType.Success
            });

            list.Add(new Notification
            {
                Id = -4,
                Title = "Weekly Roundup",
                Content = "Here’s what you missed this week.\nStay caught up!",
                CreatedOn = DateTime.Now.AddDays(-7),
                NotificationType = NotificationType.Default
            });

            list.Add(new Notification
            {
                Id = -5,
                Title = "Monthly Summary",
                Content = "Your monthly usage report is ready.\nIt looks good!",
                CreatedOn = DateTime.Now.AddMonths(-1),
                NotificationType = NotificationType.Info
            });

            for (int i = 6; i <= 15; i++)
            {
                var daysAgo = rand.Next(60, 730);
                var minutesOffset = rand.Next(0, 1440);

                list.Add(new Notification
                {
                    Id = -i,
                    Title = titles[rand.Next(titles.Length)],
                    Content = contents[rand.Next(contents.Length)],
                    CreatedOn = DateTime.Now.AddDays(-daysAgo).AddMinutes(-minutesOffset),
                    NotificationType = types[rand.Next(types.Length)]
                });
            }

            return list;
        }

    }
}