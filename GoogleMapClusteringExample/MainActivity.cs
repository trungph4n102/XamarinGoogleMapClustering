using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering;

namespace GoogleMapClusteringExample
{
	[Activity(Label = "GoogleMapClustering", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : FragmentActivity, ClusterManager.IOnClusterClickListener, ClusterManager.IOnClusterItemClickListener
	{
		int count = 1;
		private GoogleMap _map;
		private SupportMapFragment _mapFragment;
		private ClusterManager _clusterManager;


		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

			InitMapFragment();

		}

		protected override void OnResume()
		{
			base.OnResume();
			SetupMapIfNeeded();
		}

		private void InitMapFragment()
		{
			_mapFragment = SupportFragmentManager.FindFragmentByTag("map") as SupportMapFragment;
			if (_mapFragment == null)
			{
				GoogleMapOptions mapOptions = new GoogleMapOptions()
					.InvokeMapType(GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled(true)
					.InvokeMapToolbarEnabled(true)
					.InvokeZoomGesturesEnabled(true)
					.InvokeRotateGesturesEnabled(true)
					.InvokeCompassEnabled(true);

				var fragTx = SupportFragmentManager.BeginTransaction();
				_mapFragment = SupportMapFragment.NewInstance(mapOptions);
				fragTx.Add(Resource.Id.map, _mapFragment, "map");
				fragTx.Commit();
			}
		}

		private void SetupMapIfNeeded()
		{
			if (_map == null)
			{
				_map = _mapFragment.Map;
				if (_map != null)
				{
					SetViewPoint(new LatLng(47.59978, -122.3346), false);

					_clusterManager = new ClusterManager(this, _map);
					_clusterManager.SetOnClusterClickListener(this);
					_clusterManager.SetOnClusterItemClickListener(this);
					_map.SetOnCameraChangeListener(_clusterManager);
					_map.SetOnMarkerClickListener(_clusterManager);

					AddClusterItems();
				}
			}
		}

		private void AddClusterItems()
		{
			double lat = 47.59978;
			double lng = -122.3346;

			var items = new List<ClusterItem>();

			// Create a log. spiral of markers to test clustering
			for (int i = 0; i < 20; ++i)
			{
				var t = i * Math.PI * 0.33f;
				var r = 0.005 * Math.Exp(0.1 * t);
				var x = r * Math.Cos(t);
				var y = r * Math.Sin(t);
				var item = new ClusterItem(lat + x, lng + y);
				items.Add(item);
			}
			_clusterManager.AddItems(items);
		}

		public void SetViewPoint(LatLng latlng, bool animated)
		{
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(latlng);
			builder.Zoom(12f);
			CameraPosition cameraPosition = builder.Build();

			if (animated)
			{
				_map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
			}
			else
			{
				_map.MoveCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
			}
		}

		//Cluster override methods
		public bool OnClusterClick(ICluster cluster)
		{
			Toast.MakeText(this, cluster.Items.Count + " items in cluster", ToastLength.Short).Show();
			return false;
		}

		public bool OnClusterItemClick(Java.Lang.Object marker)
		{
			Toast.MakeText(this, "Marker clicked", ToastLength.Short).Show();
			return false;
		}
	}
}


