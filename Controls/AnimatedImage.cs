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

		/// <summary>
		/// Get or set the repeatBehavior of the animation when source is gif format.
		/// </summary>
		public RepeatBehavior AnimationRepeatBehavior {
			get { return (RepeatBehavior)GetValue(AnimationRepeatBehaviorProperty); }
			set { SetValue(AnimationRepeatBehaviorProperty, value); }
		}

		public new ImageSource Source {
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public Uri UriSource {
			get { return (Uri)GetValue(UriSourceProperty); }
			set { SetValue(UriSourceProperty, value); }
		}

		#endregion

		#region Protected interface

		/// <summary> 
		/// Provides derived classes an opportunity to handle changes to the Source property. 
		/// </summary> 
		protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e) {
			ClearAnimation();
			BitmapImage source;
			if (e.NewValue is Uri) {
				source = new BitmapImage();
				source.BeginInit();
				source.UriSource = e.NewValue as Uri;
				source.CacheOption = BitmapCacheOption.OnLoad;
				source.EndInit();
			} else if (e.NewValue is BitmapImage) {
				source = e.NewValue as BitmapImage;
			} else {
				return;
			}
			BitmapDecoder decoder;
			if (source.StreamSource != null) {
				decoder = BitmapDecoder.Create(source.StreamSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			} else if (source.UriSource != null) {
				decoder = BitmapDecoder.Create(source.UriSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			} else {
				return;
			}
			if (decoder.Frames.Count == 1) {
				base.Source = decoder.Frames[0];
				return;
			}

			this.Frames = decoder.Frames;

			PrepareAnimation();
		}

		#endregion

		#region Private properties

		private Int32Animation Animation { get; set; }
		private bool IsAnimationWorking { get; set; }

		#endregion

		#region Private methods

		private void ClearAnimation() {
			if (Animation != null) {
				BeginAnimation(FrameIndexProperty, null);
			}

			IsAnimationWorking = false;
			Animation = null;
			this.Frames = null;
		}

		private void PrepareAnimation() {
			Animation =
				new Int32Animation(
					0,
					this.Frames.Count - 1,
					new Duration(
						new TimeSpan(
							0,
							0,
							0,
							this.Frames.Count / 10,
							(int)((this.Frames.Count / 10.0 - this.Frames.Count / 10) * 1000)))) {
								RepeatBehavior = RepeatBehavior.Forever
							};

			base.Source = this.Frames[0];
			BeginAnimation(FrameIndexProperty, Animation);
			IsAnimationWorking = true;
		}

		private static void ChangingFrameIndex
			(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
			AnimatedImage animatedImage = dp as AnimatedImage;

			if (animatedImage == null || !animatedImage.IsAnimationWorking) {
				return;
			}

			int frameIndex = (int)e.NewValue;
			((Image)animatedImage).Source = animatedImage.Frames[frameIndex];
			animatedImage.InvalidateVisual();
		}

		/// <summary> 
		/// Handles changes to the Source property. 
		/// </summary> 
		private static void OnSourceChanged
			(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
			((AnimatedImage)dp).OnSourceChanged(e);
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty FrameIndexProperty =
			DependencyProperty.Register(
				"FrameIndex",
				typeof(int),
				typeof(AnimatedImage),
				new UIPropertyMetadata(0, ChangingFrameIndex));

		public new static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(
				"Source",
				typeof(ImageSource),
				typeof(AnimatedImage),
				new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.AffectsRender |
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					OnSourceChanged));

		public static readonly DependencyProperty AnimationRepeatBehaviorProperty =
			DependencyProperty.Register(
			"AnimationRepeatBehavior",
			typeof(RepeatBehavior),
			typeof(AnimatedImage),
			new PropertyMetadata(null));

		public static readonly DependencyProperty UriSourceProperty =
			DependencyProperty.Register(
			"UriSource",
			typeof(Uri),
			typeof(AnimatedImage),
					new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.AffectsRender |
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					OnSourceChanged));

		#endregion
	}
}
