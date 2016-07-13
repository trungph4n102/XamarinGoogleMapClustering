using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering;

namespace GoogleMapClusteringExample
{
	public class ClusterItem : Java.Lang.Object, IClusterItem
	{
		public ClusterItem()
		{
		}
		public LatLng Position { get; set; }

		public ClusterItem(double lat, double lng)
		{
			Position = new LatLng(lat, lng);
		}
	}
}
