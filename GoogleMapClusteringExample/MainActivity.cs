using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;

namespace GoogleMapClusteringExample
{
	[Activity(Label = "GoogleMapClustering", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : FragmentActivity, IOnMapReadyCallback, ILocationListener, ClusterManager.IOnClusterClickListener, ClusterManager.IOnClusterItemClickListener
	{
		const string TAG = "GoogleMapClustering";
		Location _currentLocation;
		LocationManager _locationManager;
		string _locationProvider;
		SupportMapFragment _mapFragment;
		ClusterManager _clusterManager;
		GoogleMap _googleMap;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);

			PackageInfo pInfo = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.MetaData);
			var md = pInfo.ApplicationInfo.MetaData;
			if (md.ContainsKey("com.google.android.maps.v2.API_KEY"))
			{
				var apiKey = md.GetString("com.google.android.maps.v2.API_KEY");
				if (apiKey == "YourMapSDKKeyHere")
					throw new Exception("You need to assign a Google Map API Key in the manifest");
			}
			else
				throw new Exception("com.google.android.maps.v2.API_KEY misssing from the manifest");

			InitializeLocationManager();
		}

		protected override void OnResume()
		{
			base.OnResume();
			SetupMapIfNeeded();
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
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
			if (_mapFragment == null)
				InitMapFragment();
			_mapFragment?.GetMapAsync(this);
		}

		private void AddClusterItems(Location currentLocation)
		{

			var items = new List<ClusterItem>();

			// Add current location to the cluster list
			var currentMarker = new MarkerOptions();
			var me = new LatLng(currentLocation.Latitude, currentLocation.Longitude);
			currentMarker.SetPosition(me);
			var meMarker = new CircleOptions();
			meMarker.InvokeCenter(me);
			meMarker.InvokeRadius(32);
			meMarker.InvokeStrokeWidth(0);
			meMarker.InvokeFillColor(ContextCompat.GetColor(BaseContext, Android.Resource.Color.HoloBlueLight));
			_googleMap.AddCircle(meMarker);
			items.Add(new ClusterItem(currentLocation.Latitude, currentLocation.Longitude));

			// Create a log. spiral of markers to test clustering
			for (int i = 0; i < 20; ++i)
			{
				var t = i * Math.PI * 0.33f;
				var r = 0.005 * Math.Exp(0.1 * t);
				var x = r * Math.Cos(t);
				var y = r * Math.Sin(t);
				items.Add(new ClusterItem(currentLocation.Latitude + x, currentLocation.Longitude + y));
			}
			_clusterManager.AddItems(items);
		}

		public void SetViewPoint(GoogleMap googleMap, LatLng latlng, bool animated)
		{
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(latlng);
			builder.Zoom(12f);
			CameraPosition cameraPosition = builder.Build();

			if (animated)
				googleMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
			else
				googleMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
		}

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

		public void SetupMapClustingDemo()
		{
			if (_googleMap == null || _currentLocation == null) // Do we have a map and location avaialble?
				return;
			_clusterManager = new ClusterManager(this, _googleMap);
			_clusterManager.SetOnClusterClickListener(this);
			_clusterManager.SetOnClusterItemClickListener(this);
			_googleMap.SetOnCameraChangeListener(_clusterManager);
			_googleMap.SetOnMarkerClickListener(_clusterManager);

			SetViewPoint(_googleMap, new LatLng(_currentLocation.Latitude, _currentLocation.Longitude), true);
			AddClusterItems(_currentLocation);
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			_googleMap = googleMap;
			SetupMapClustingDemo();
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			var criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = string.Empty;
			}
			Log.Debug(TAG, "Using " + _locationProvider + ".");
			_locationManager?.RequestLocationUpdates(_locationProvider, 0, 0, this);
		}

		public void OnLocationChanged(Location location)
		{
			Log.Debug(TAG, "OnLocationChanged");
			_currentLocation = location;
			if (_currentLocation == null)
			{
				Toast.MakeText(BaseContext, "Unable to determine your location, using Seattle", ToastLength.Long).Show();
				_currentLocation = new Location(string.Empty)
				{
					Latitude = 47.59978,
					Longitude = -122.3346
				};
			}
			_locationManager.RemoveUpdates(this); // just a one-shot location update for this demo
			SetupMapClustingDemo();
		}

		public void OnProviderDisabled(string provider)
		{
			Log.Debug(TAG, "OnProviderDisabled");
		}

		public void OnProviderEnabled(string provider)
		{
			Log.Debug(TAG, "OnProviderEnabled");
		}

		public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
		{
			Log.Debug(TAG, "OnStatusChanged");
			_locationManager?.RemoveUpdates(this);
		}
	}
}


