using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace LibraryBrowser.Controls {
	public class AnimatedImage : Image {
		static AnimatedImage() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
			StretchProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(Stretch.None));
			SnapsToDevicePixelsProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(true));
		}

		#region Public properties
		public int FrameIndex {
			get { return (int)GetValue(FrameIndexProperty); }
			set { SetValue(FrameIndexProperty, value); }
		}

		public ReadOnlyCollection<BitmapFrame> Frames { get; private set; }

		public new ImageSource Source {
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}
		#endregion


		static BitmapDecoder GetDecoder(object value) {
			var frame = value as BitmapFrame;
			if (frame != null)
				return frame.Decoder;

			BitmapImage source = value as BitmapImage;
			if (source == null)
				return null;

			if (source.StreamSource != null)
				return BitmapDecoder.Create(source.StreamSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			if (source.UriSource != null)
				return BitmapDecoder.Create(source.UriSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			return null;
		}

		protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e) {
			ClearAnimation();

			if (e.NewValue == null || e.NewValue == DependencyProperty.UnsetValue)
				return;

			var decoder = GetDecoder(e.NewValue);
			if (decoder == null)
				throw new InvalidOperationException(e.NewValue + " (" + e.NewValue.GetType() + ") is not an image");

			if (decoder.Frames.Count == 1)
				base.Source = decoder.Frames[0];
			else {
				this.Frames = decoder.Frames;
				PrepareAnimation();
			}
		}

		private Int32Animation Animation { get; set; }

		#region Private methods
		private void ClearAnimation() {
			if (Animation != null)
				BeginAnimation(FrameIndexProperty, null);

			Animation = null;
			this.Frames = null;
		}

		private void PrepareAnimation() {
			Animation = new Int32Animation(0, this.Frames.Count - 1, new Duration(TimeSpan.FromMilliseconds(Frames.Count * 100))) {
				RepeatBehavior = RepeatBehavior.Forever
			};

			base.Source = this.Frames[0];
			BeginAnimation(FrameIndexProperty, Animation);
		}

		private static void OnFrameIndexChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
			AnimatedImage animatedImage = dp as AnimatedImage;

			if (animatedImage == null)
				return;

			int frameIndex = (int)e.NewValue;
			((Image)animatedImage).Source = animatedImage.Frames[frameIndex];
			//animatedImage.InvalidateVisual();
		}

		private static void OnSourceChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
			((AnimatedImage)dp).OnSourceChanged(e);
		}
		#endregion

		#region Dependency Properties
		public static readonly DependencyProperty FrameIndexProperty =
			DependencyProperty.Register("FrameIndex", typeof(int), typeof(AnimatedImage), new UIPropertyMetadata(0, OnFrameIndexChanged));

		public new static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(ImageSource), typeof(AnimatedImage),
				new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.AffectsRender |
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					OnSourceChanged
				)
			);
		#endregion
	}
}
