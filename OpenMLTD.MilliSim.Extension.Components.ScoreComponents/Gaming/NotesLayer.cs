using System;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using OpenMLTD.MilliSim.Core;
using OpenMLTD.MilliSim.Core.Entities;
using OpenMLTD.MilliSim.Core.Entities.Extensions;
using OpenMLTD.MilliSim.Core.Entities.Runtime;
using OpenMLTD.MilliSim.Extension.Components.CoreComponents;
using OpenMLTD.MilliSim.Extension.Components.ScoreComponents.Configuration;
using OpenMLTD.MilliSim.GameAbstraction.Extensions;
using OpenMLTD.MilliSim.Graphics;
using OpenMLTD.MilliSim.Graphics.Drawing;
using OpenMLTD.MilliSim.Graphics.Drawing.Direct2D;
using OpenMLTD.MilliSim.Graphics.Drawing.Direct2D.Advanced;
using OpenMLTD.MilliSim.Graphics.Extensions;
using OpenMLTD.MilliSim.Theater.Animation;
using OpenMLTD.MilliSim.Theater.Animation.Extending;

namespace OpenMLTD.MilliSim.Extension.Components.ScoreComponents.Gaming {
    public class NotesLayer : BufferedVisual2D {

        public NotesLayer([NotNull] IVisualContainer parent)
            : base(parent) {
        }

        /// <summary>
        /// Note falling speed scale. The same one used in game.
        /// </summary>
        public float GlobalSpeedScale {
            get => _globalSpeedScale;
            set {
                if (value <= 0.05f) {
                    value = 0.05f;
                }
                _globalSpeedScale = value;
            }
        }

        internal INoteTraceCalculator TraceCalculator { get; private set; }

        internal static (int ImageIndex, bool IsHugeNote) GetImageIndex(NoteType type, NoteSize size, FlickDirection flickDirection, bool isHoldStart, bool isHoldEnd, bool isSlideStart, bool isSlideEnd) {
            if (isHoldStart && isHoldEnd) {
                throw new ArgumentException();
            }
            if (isSlideStart && isSlideEnd) {
                throw new ArgumentException();
            }
            if ((isHoldStart || isHoldEnd) && (isSlideStart || isSlideEnd)) {
                throw new ArgumentException();
            }

            var imageIndex = -1;
            var isHugeNote = false;
            switch (type) {
                case NoteType.Tap:
                    switch (size) {
                        case NoteSize.Small:
                            imageIndex = 0;
                            break;
                        case NoteSize.Large:
                            imageIndex = 1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case NoteType.Flick:
                    switch (flickDirection) {
                        case FlickDirection.Left:
                            imageIndex = 4;
                            break;
                        case FlickDirection.Up:
                            imageIndex = 5;
                            break;
                        case FlickDirection.Right:
                            imageIndex = 6;
                            break;
                        case FlickDirection.Down:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case NoteType.Hold:
                    switch (flickDirection) {
                        case FlickDirection.None:
                        case FlickDirection.Down:
                            switch (size) {
                                case NoteSize.Small:
                                    imageIndex = 2;
                                    break;
                                case NoteSize.Large:
                                    imageIndex = 3;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case FlickDirection.Left:
                            imageIndex = 4;
                            break;
                        case FlickDirection.Up:
                            imageIndex = 5;
                            break;
                        case FlickDirection.Right:
                            imageIndex = 6;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case NoteType.Slide:
                    switch (flickDirection) {
                        case FlickDirection.None:
                        case FlickDirection.Down:
                            if (isSlideStart) {
                                imageIndex = 7;
                            } else if (isSlideEnd) {
                                imageIndex = 9;
                            } else {
                                imageIndex = 8;
                            }
                            break;
                        case FlickDirection.Left:
                            imageIndex = 4;
                            break;
                        case FlickDirection.Up:
                            imageIndex = 5;
                            break;
                        case FlickDirection.Right:
                            imageIndex = 6;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case NoteType.Special:
                    isHugeNote = true;
                    break;
                case NoteType.SpecialEnd:
                case NoteType.SpecialPrepare:
                case NoteType.ScorePrepare:
                    // We don't draw these notes.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (imageIndex, isHugeNote);
        }

        protected override void OnDrawBuffer(GameTime gameTime, RenderContext context) {
            base.OnDrawBuffer(gameTime, context);

            if (_score == null) {
                return;
            }

            if (_noteImages?[0] == null) {
                return;
            }

            var noteImages = _noteImages;
            var notes = _score.Notes;

            var theaterDays = Game.AsTheaterDays();

            var syncTimer = theaterDays.FindSingleElement<SyncTimer>();
            if (syncTimer == null) {
                return;
            }

            var tapPoints = theaterDays.FindSingleElement<TapPoints>();
            if (tapPoints == null) {
                throw new InvalidOperationException();
            }

            var scalingResponder = theaterDays.FindSingleElement<MltdStageScalingResponder>();
            if (scalingResponder == null) {
                throw new InvalidOperationException();
            }

            var now = syncTimer.CurrentTime.TotalSeconds;

            var tapPointsConfig = ConfigurationStore.Get<TapPointsConfig>();
            var notesLayerConfig = ConfigurationStore.Get<NotesLayerConfig>();
            var tapPointsLayout = tapPointsConfig.Data.Layout;
            var notesLayerLayout = notesLayerConfig.Data.Layout;
            var clientSize = context.ClientSize;

            var animationMetrics = new NoteAnimationMetrics {
                GlobalSpeedScale = GlobalSpeedScale,
                Width = clientSize.Width,
                Height = clientSize.Height,
                Top = notesLayerLayout.Y.IsPercentage ? notesLayerLayout.Y.Value * clientSize.Height : notesLayerLayout.Y.Value,
                Bottom = tapPointsLayout.Y.IsPercentage ? tapPointsLayout.Y.Value * clientSize.Height : tapPointsLayout.Y.Value,
                NoteStartXRatios = tapPoints.StartXRatios,
                NoteEndXRatios = tapPoints.EndXRatios,
                TrackCount = tapPoints.EndXRatios.Length
            };

            var commonNoteMetrics = new NoteMetrics {
                StartRadius = scalingResponder.ScaleResults.Note.Start,
                EndRadius = scalingResponder.ScaleResults.Note.End
            };

            var specialNoteMetrics = new NoteMetrics {
                StartRadius = scalingResponder.ScaleResults.SpecialNote.Start,
                EndRadius = scalingResponder.ScaleResults.SpecialNote.End
            };

            context.Begin2D();

            foreach (var note in notes) {
                if (!NoteAnimationHelper.IsNoteVisible(note, now, animationMetrics)) {
                    continue;
                }

                var (imageIndex, isHugeNote) = GetImageIndex(note);

                if (imageIndex < 0 && !isHugeNote) {
                    continue;
                }

                if (!isHugeNote) {
                    var thisX = TraceCalculator.GetNoteX(note, now, commonNoteMetrics, animationMetrics);
                    var thisY = TraceCalculator.GetNoteY(note, now, commonNoteMetrics, animationMetrics);
                    var thisRadius = TraceCalculator.GetNoteRadius(note, now, commonNoteMetrics, animationMetrics);

                    if (notesLayerConfig.Data.Style.SyncLine) {
                        // NextSync is always after PrevSync. So draw here, below the two notes.
                        if (note.HasNextSync()) {
                            var drawSyncLine = true;
                            if (note.IsSlideMiddle() || note.NextSync.IsSlideMiddle()) {
                                drawSyncLine = notesLayerConfig.Data.Style.SlideMiddleSyncLine;
                            }
                            if (drawSyncLine) {
                                var nextX = TraceCalculator.GetNoteX(note.NextSync, now, commonNoteMetrics, animationMetrics);
                                context.DrawLine(_simpleSyncLinePen, thisX, thisY, nextX, thisY);
                            }
                        }
                    }

                    var opacity = notesLayerConfig.Data.Opacity.Value;

                    // Then draw the note.
                    context.DrawImageStripUnit(noteImages[0], imageIndex, thisX - thisRadius.Width / 2, thisY - thisRadius.Height / 2, thisRadius.Width, thisRadius.Height, opacity);
                } else {
                    if (_specialNoteImage != null) {
                        var thisX = TraceCalculator.GetSpecialNoteX(note, now, specialNoteMetrics, animationMetrics);
                        var thisY = TraceCalculator.GetSpecialNoteY(note, now, specialNoteMetrics, animationMetrics);
                        var thisRadius = TraceCalculator.GetSpecialNoteRadius(note, now, specialNoteMetrics, animationMetrics);
                        context.DrawBitmap(_specialNoteImage, thisX - thisRadius.Width / 2, thisY - thisRadius.Height / 2, thisRadius.Width, thisRadius.Height);
                    }
                }
            }

            context.End2D();
        }

        protected override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);

            var theaterDays = Game.AsTheaterDays();
            var debugOverlay = theaterDays.FindSingleElement<DebugOverlay>();
            var config = ConfigurationStore.Get<NotesLayerConfig>();

            var scalingResponder = theaterDays.FindSingleElement<MltdStageScalingResponder>();
            if (scalingResponder == null) {
                throw new InvalidOperationException();
            }

            if (config.Data.Images.Notes == null || config.Data.Images.Notes.Length == 0) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine("WARNING: default notes image strip is not specified.");
                }
            } else {
                _noteImages = new D2DImageStrip[config.Data.Images.Notes.Length];

                for (var i = 0; i < config.Data.Images.Notes.Length; ++i) {
                    var notesImageEntry = config.Data.Images.Notes[i];
                    if (notesImageEntry == null) {
                        continue;
                    }

                    if (notesImageEntry.File == null || !File.Exists(notesImageEntry.File)) {
                        if (i == 0) {
                            if (debugOverlay != null) {
                                debugOverlay.AddLine($"WARNING: default notes image strip <{notesImageEntry.File ?? string.Empty}> is not found.");
                            }
                        } else {
                            if (debugOverlay != null) {
                                debugOverlay.AddLine($"WARNING: notes image strip <{notesImageEntry.File ?? string.Empty}> is not found, falling back to default.");
                            }
                        }
                        continue;
                    }

                    var imageStrip = Direct2DHelper.LoadImageStrip(context, notesImageEntry.File, notesImageEntry.Count, (ImageStripOrientation)notesImageEntry.Orientation);
                    _noteImages[i] = imageStrip;
                }
            }

            if (config.Data.Images.SpecialNote.FileName == null) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine("WARNING: huge note image is not specified.");
                }
            } else if (!File.Exists(config.Data.Images.SpecialNote.FileName)) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine($"WARNING: huge note image file <{config.Data.Images.SpecialNote.FileName}> is not found.");
                }
            } else {
                _specialNoteImage = Direct2DHelper.LoadBitmap(context, config.Data.Images.SpecialNote.FileName);
            }

            _simpleSyncLinePen = new D2DPen(context, Color.White, scalingResponder.ScaleResults.SyncLine.Width);
        }

        protected override void OnLostContext(RenderContext context) {
            base.OnLostContext(context);

            _simpleSyncLinePen.Dispose();

            if (_noteImages != null) {
                foreach (var noteImage in _noteImages) {
                    noteImage?.Dispose();
                }
            }
            _noteImages = null;

            _specialNoteImage?.Dispose();
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            var scoreLoader = Game.AsTheaterDays().FindSingleElement<ScoreLoader>();
            _score = scoreLoader?.RuntimeScore;

            var theaterDays = Game.AsTheaterDays();
            var config = ConfigurationStore.Get<NotesLayerConfig>();

            var traceCalculators = theaterDays.PluginManager.GetPluginsOfType<INoteTraceCalculator>();
            if (traceCalculators.Count == 0) {
                throw new InvalidOperationException("You need at least 1 note trace calculator.");
            }

            var debugOverlay = Game.AsTheaterDays().FindSingleElement<DebugOverlay>();

            var calculator = traceCalculators.FirstOrDefault(calc => calc.PluginID == config.Data.Style.TracePluginID);
            if (calculator == null) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine($"Missing note trace calculator with plugin ID '{config.Data.Style.TracePluginID}'. Using the first loaded calculator '{traceCalculators[0].PluginID}'.");
                }
                calculator = traceCalculators[0];
            }

            TraceCalculator = calculator;

            GlobalSpeedScale = config.Data.Simulation.GlobalSpeed.Value;
        }

        private static (int ImageIndex, bool IsHugeNote) GetImageIndex(RuntimeNote note) {
            return GetImageIndex(note.Type, note.Size, note.FlickDirection, note.IsHoldStart(), note.IsHoldEnd(), note.IsSlideStart(), note.IsSlideEnd());
        }

        [CanBeNull]
        private D2DBitmap _specialNoteImage;

        [CanBeNull, ItemCanBeNull]
        private D2DImageStrip[] _noteImages;

        [CanBeNull]
        private RuntimeScore _score;

        private D2DPen _simpleSyncLinePen;

        private float _globalSpeedScale = 1f;

    }
}
