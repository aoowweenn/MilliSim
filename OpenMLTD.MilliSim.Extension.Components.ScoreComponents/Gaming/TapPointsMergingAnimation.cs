using System;
using JetBrains.Annotations;
using OpenMLTD.MilliSim.Core;
using OpenMLTD.MilliSim.Extension.Components.CoreComponents;
using OpenMLTD.MilliSim.Extension.Components.ScoreComponents.Configuration;
using OpenMLTD.MilliSim.GameAbstraction.Extensions;
using OpenMLTD.MilliSim.Graphics;
using OpenMLTD.MilliSim.Graphics.Drawing;
using OpenMLTD.MilliSim.Graphics.Drawing.Direct2D;
using OpenMLTD.MilliSim.Graphics.Extensions;

namespace OpenMLTD.MilliSim.Extension.Components.ScoreComponents.Gaming {
    public class TapPointsMergingAnimation : Visual2D {

        public TapPointsMergingAnimation([NotNull] IVisualContainer parent)
            : base(parent) {
        }

        public void StartAnimation() {
            var syncTimer = Game.AsTheaterDays().FindSingleElement<SyncTimer>();
            if (syncTimer == null) {
                throw new InvalidOperationException();
            }

            _animationStartedTime = syncTimer.CurrentTime;
            _isAnimationStarted = true;
        }

        protected override void OnDraw(GameTime gameTime, RenderContext context) {
            base.OnDraw(gameTime, context);

            if (!_isAnimationStarted) {
                return;
            }

            var theaterDays = Game.AsTheaterDays();

            var syncTimer = theaterDays.FindSingleElement<SyncTimer>();
            if (syncTimer == null) {
                throw new InvalidOperationException();
            }

            var scalingResponder = theaterDays.FindSingleElement<MltdStageScalingResponder>();
            if (scalingResponder == null) {
                throw new InvalidOperationException();
            }

            var tapPoints = theaterDays.FindSingleElement<TapPoints>();
            if (tapPoints == null) {
                throw new InvalidOperationException();
            }

            var currentTime = syncTimer.CurrentTime;
            if (currentTime < _animationStartedTime) {
                // Automatically cancels the animation if the user steps back in UI.
                _isAnimationStarted = false;
                return;
            }

            var animationTime = (currentTime - _animationStartedTime).TotalSeconds;

            if (animationTime > _phase1Duration + _phase2Duration) {
                _isAnimationStarted = false;
                return;
            }

            var clientSize = context.ClientSize;
            var scalingResults = scalingResponder.ScaleResults;
            var tapPointsConfig = ConfigurationStore.Get<TapPointsConfig>();
            var tapPointsLayout = tapPointsConfig.Data.Layout;

            context.Begin2D();

            float perc;
            var y = tapPointsLayout.Y.IsPercentage ? tapPointsLayout.Y.Value * clientSize.Height : tapPointsLayout.Y.Value;
            if (animationTime <= _phase1Duration) {
                perc = (float)animationTime / (float)_phase1Duration;

                var tapPointSizes = scalingResults.TapPoint;
                var auraSizes = scalingResults.SpecialNoteAura;

                var tapPointWidth = MathHelper.Lerp(tapPointSizes.Start.Width, tapPointSizes.End.Width, perc);
                var tapPointHeight = MathHelper.Lerp(tapPointSizes.Start.Height, tapPointSizes.End.Height, perc);
                var auraWidth = MathHelper.Lerp(auraSizes.Start.Width, auraSizes.End.Width, perc);
                var auraHeight = MathHelper.Lerp(auraSizes.Start.Height, auraSizes.End.Height, perc);

                var xRatioArray = tapPoints.EndXRatios;
                for (var i = 0; i < xRatioArray.Length; ++i) {
                    var x = MathHelper.Lerp(xRatioArray[i], 0.5f, perc) * clientSize.Width;
                    context.DrawBitmap(_tapPointImage, x - tapPointWidth / 2, y - tapPointHeight / 2, tapPointWidth, tapPointHeight, 1 - perc);
                    context.DrawBitmap(_auraImage, x - auraWidth / 2, y - auraHeight / 2, auraWidth, auraHeight, perc);
                }
            } else {
                perc = (float)(animationTime - _phase1Duration) / (float)_phase2Duration;

                var auraSize = scalingResults.SpecialNoteAura.End;
                var socketSize = scalingResults.SpecialNoteSocket;

                var x = clientSize.Width * 0.5f;
                context.DrawBitmap(_socketImage, x - socketSize.Width / 2, y - socketSize.Width / 2, socketSize.Width, socketSize.Height, perc);
                context.DrawBitmap(_auraImage, x - auraSize.Width / 2, y - auraSize.Height / 2, auraSize.Width, auraSize.Height);
            }

            context.End2D();
        }

        protected override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);

            var notesLayerConfig = ConfigurationStore.Get<NotesLayerConfig>();
            var tapPointsConfig = ConfigurationStore.Get<TapPointsConfig>();

            _auraImage = Direct2DHelper.LoadBitmap(context, notesLayerConfig.Data.Images.SpecialNoteAura.FileName);
            _socketImage = Direct2DHelper.LoadBitmap(context, notesLayerConfig.Data.Images.SpecialNoteSocket.FileName);
            _tapPointImage = Direct2DHelper.LoadBitmap(context, tapPointsConfig.Data.Images.TapPoint.FileName);
        }

        protected override void OnLostContext(RenderContext context) {
            _auraImage.Dispose();
            _socketImage.Dispose();
            _tapPointImage.Dispose();

            base.OnLostContext(context);
        }

        // Total time: 0.8s (corresponding to the advancing time of Special Prepare)
        private readonly double _phase1Duration = 0.5;
        private readonly double _phase2Duration = 0.3;

        private D2DBitmap _auraImage;
        private D2DBitmap _socketImage;
        private D2DBitmap _tapPointImage;

        private bool _isAnimationStarted;
        private TimeSpan _animationStartedTime;

    }
}
