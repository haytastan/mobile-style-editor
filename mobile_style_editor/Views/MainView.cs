
using System;
using Carto.Core;
using Carto.DataSources;
using Carto.Layers;
using Carto.Styles;
using Carto.Ui;
using Carto.Utils;
using Carto.VectorTiles;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
#elif __ANDROID__
using Xamarin.Forms.Platform.Android;
#elif __UWP__
using Xamarin.Forms.Platform.UWP;
#endif

namespace mobile_style_editor
{
	public class MainView : BaseView
	{
		public Toolbar Toolbar { get; private set; }

		public MapView MapView { get; private set; }

		public CSSEditorView Editor { get; private set; }

		public ConfirmationPopup Popup { get; private set; }

		public FileTabPopup FileTabs { get; private set; }
		
		public MainView()
		{
			Toolbar = new Toolbar();
#if __IOS__
			MapView = new MapView();
#elif __ANDROID__
			MapView = new MapView(Forms.Context);
#elif __UWP__
            MapView = new MapView();
#endif
			Editor = new CSSEditorView();

			Popup = new ConfirmationPopup();

			FileTabs = new FileTabPopup();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			double x = 0;
			double y = 0;
			double w = Width;
			double h = Height / 12;
			double min = 50;

			if (h < min)
			{
				h = min;
			}

			AddSubview(Toolbar, x, y, w, h);

			y += h;
			w = Width / 3 * 1.9;
			h = Height - h;

			AddSubview(MapView.ToView(), x, y, w, h);

			x += w;
			w = Width - w;
			AddSubview(Editor, x, y, w, h);

			if (Data != null)
			{
				Editor.Initialize(Data);
				Toolbar.Initialize(Data);
			}
		}

		ZipData Data;

		public void Initialize(ZipData data)
		{
			Data = data;
			Editor.Initialize(Data);
			Toolbar.Initialize(Data);
			FileTabs.Initialize(this, data);

			// Set toolbar on top of file tab popup
			Children.Remove(Toolbar);
			AddSubview(Toolbar, 0, 0, Toolbar.Width, Toolbar.Height);

			// Add popup view so it would cover other views
			AddSubview(Popup, 0, 0, Width, Height);
			Popup.Hide();
		}

		public void ToggleTabs()
		{
			bool willExpand = FileTabs.Toggle();

			if (willExpand)
			{
				Toolbar.ExpandButton.UpdateText(FileTabs.CurrentHighlight);
			}

			Toolbar.ExpandButton.UpdateImage();
		}

		const string OSM = "nutiteq.osm";

		public void UpdateMap(byte[] data, Action completed)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				BinaryData styleAsset = new BinaryData(data);

				var package = new ZippedAssetPackage(styleAsset);
				var styleSet = new CompiledStyleSet(package);
			
				// UWP doesn't have a version released where simply changing the style set is supported,
				// need to clear layers and recreate the entire thing
#if __UWP__
				MapView.Layers.Clear();
		
				var source = new CartoOnlineTileDataSource(OSM);
				var decoder = new MBVectorTileDecoder(styleSet);
                
				var layer = new VectorTileLayer(source, decoder);
				Device.BeginInvokeOnMainThread(delegate
				{
					MapView.Layers.Add(layer);
                    completed();
				});
#else
				if (MapView.Layers.Count == 0)
				{
					var source = new CartoOnlineTileDataSource(OSM);
					var decoder = new MBVectorTileDecoder(styleSet);

					var layer = new VectorTileLayer(source, decoder);
					Device.BeginInvokeOnMainThread(delegate
					{
						MapView.Layers.Add(layer);
						completed();
					});
				}
				else
				{
					var decoder = (MBVectorTileDecoder)(MapView.Layers[0] as VectorTileLayer).TileDecoder;

					Device.BeginInvokeOnMainThread(delegate
					{
						decoder.CompiledStyle = styleSet;
						completed();
					});
				}
#endif
            });
		}
	}
}
