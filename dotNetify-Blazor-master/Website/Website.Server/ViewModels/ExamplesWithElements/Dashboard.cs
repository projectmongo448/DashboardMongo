﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DotNetify;
using DotNetify.Elements;
using DotNetify.Routing;

namespace Website.Server
{
    public class Dashboard : BaseVM, IRoutable
    {
        private IDisposable _subscription;

        public RoutingState RoutingState { get; set; } = new RoutingState();

        public Dashboard(ILiveDataService liveDataService)
        {
            AddProperty<string>("Download")
               .WithAttribute(new { Label = "แรงดันไฟฟ้า(Volt)", Icon = "flash_on" })
               .SubscribeTo(liveDataService.Download);

            AddProperty<string>("Upload")
               .WithAttribute(new { Label = "แรงบิด(Rotate)", Icon = "crop" })
               .SubscribeTo(liveDataService.Upload);

            AddProperty<string>("Latency")
               .WithAttribute(new { Label = "แรงดัน(Pressure)", Icon = "tonality" })
               .SubscribeTo(liveDataService.Latency);

            AddProperty<string>("Users")
               .WithAttribute(new { Label = "แรงสั่นสะเทือน(Vibration)", Icon = "style" })
               .SubscribeTo(liveDataService.Users);

            AddProperty<int[]>("Traffic")
                .WithAttribute(new ChartAttribute { Labels = new string[] { "แรงดันไฟฟ้า", "แรงบิด" } })
               .SubscribeTo(liveDataService.Traffic);

            AddProperty<int[]>("Utilization")
               .WithAttribute(new ChartAttribute { Labels = new string[] { "แรงดันไฟฟ้า", "แรงบิด", "แรงดัน", "แรงสั่นสะเทือน" } })
               .SubscribeTo(liveDataService.Utilization);

            AddProperty<int[]>("ServerUsage").SubscribeTo(liveDataService.ServerUsage)
               .WithAttribute(new ChartAttribute { Labels = new string[] { "แรงดันไฟฟ้า", "แรงบิด", "แรงดัน", "แรงสั่นสะเทือน" } });

            AddProperty<Activity[]>("RecentActivities")
               .SubscribeTo(liveDataService.RecentActivity.Select(value =>
               {
                   var activities = new Queue<Activity>(Get<Activity[]>("RecentActivities")?.Reverse() ?? new Activity[] { });
                   activities.Enqueue(value);
                   if (activities.Count > 4)
                       activities.Dequeue();
                   return activities.Reverse().ToArray();
               }));

            // Regulate data update interval to no less than every 200 msecs.
            _subscription = Observable
               .Interval(TimeSpan.FromMilliseconds(200))
               .StartWith(0)
               .Subscribe(_ => PushUpdates());
        }

        public override void Dispose()
        {
            _subscription?.Dispose();
            base.Dispose();
        }
    }
}