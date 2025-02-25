﻿using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Widget;

using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Android.Content;
using AndroidX.LocalBroadcastManager.Content;
using Android.Gms.Common.Api.Internal;
using Android.Gms.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Step1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity,
        MessageClient.IOnMessageReceivedListener,
        CapabilityClient.IOnCapabilityChangedListener
    {
        int count = 0;
        TextView textViewDisplay;
        ICollection<INode> nodes;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            textViewDisplay = FindViewById<TextView>(Resource.Id.text_view_display);

            var decButton = FindViewById<Button>(Resource.Id.dec_button);
            decButton.Click += (object sender, EventArgs e) =>
            {
                --count;
                UpdateTextDisplay();
                SyncCountToWatch();
            };

            var incButton = FindViewById<Button>(Resource.Id.inc_button);
            incButton.Click += (object sender, EventArgs e) =>
            {
                ++count;
                UpdateTextDisplay();
                SyncCountToWatch();
            };

            SetupCapability();
        }

        async Task SetupCapability()
        {
            await WearableClass.GetMessageClient(this).AddListenerAsync(this);

            var capabilityInfo = await WearableClass.GetCapabilityClient(this).GetCapabilityAsync("step1_sync_count", CapabilityClient.FilterReachable);
            nodes = capabilityInfo.Nodes;

            await WearableClass.GetCapabilityClient(this).AddListenerAsync(this, "step1_sync_count");
        }


        void UpdateTextDisplay()
        {
            RunOnUiThread(() =>
            {
                textViewDisplay.Text = count.ToString();
            });
        }

        async Task SyncCountToWatch()
        {
            var data = BitConverter.GetBytes(count);
            //await UpdateConnectedNodes();
            System.Diagnostics.Debug.WriteLine($"Phone: Attempting to send to {nodes.Count} nodes");
            foreach (var node in nodes)
            {
                System.Diagnostics.Debug.WriteLine($"Phone: Attempting to send to {node.DisplayName}");
                var result = await WearableClass.GetMessageClient(this).SendMessageAsync(node.Id, "/count", data);
                System.Diagnostics.Debug.WriteLine(result);
            }
        }

        /*
        async Task<ICollection<INode>> UpdateConnectedNodes()
        {
            var capabilityInfo = await WearableClass.GetCapabilityClient(this).GetCapabilityAsync("step1_sync_count", CapabilityClient.FilterReachable);
            System.Diagnostics.Debug.WriteLine($"UpdateConnectedNodes: Found {capabilityInfo.Nodes.Count}");
            foreach (var node in capabilityInfo.Nodes)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateConnectedNodes: {node.DisplayName}");
            }
            nodes = await WearableClass.GetNodeClient(this).GetConnectedNodesAsync();
            return nodes;
        }
        */



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        #region CapabilityClient.IOnCapabilityChangedListener
        public void OnCapabilityChanged(ICapabilityInfo capabilityInfo)
        {
            System.Diagnostics.Debug.WriteLine("Phone: OnCapabilityChanged");
            nodes = capabilityInfo.Nodes;
        }
        #endregion

        #region MessageClient.IOnMessageReceivedListener
        public void OnMessageReceived(IMessageEvent messageEvent)
        {
            System.Diagnostics.Debug.WriteLine("Phone: OnMessageReceived");
            if (messageEvent.Path == "/count")
            {
                var data = messageEvent.GetData();
                count = BitConverter.ToInt32(data);
                UpdateTextDisplay();
            }
        }
        #endregion



    }
}
