using System;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using OpenMLTD.MilliSim.Core;
using OpenMLTD.MilliSim.Core.Entities.Runtime;
using OpenMLTD.MilliSim.Core.Entities.Runtime.Extensions;
using OpenMLTD.MilliSim.Foundation;
using OpenMLTD.MilliSim.Graphics;
using OpenMLTD.MilliSim.Graphics.Drawing;
using OpenMLTD.MilliSim.Graphics.Drawing.Direct2D;
using OpenMLTD.MilliSim.Graphics.Drawing.Direct2D.Advanced;
using OpenMLTD.MilliSim.Graphics.Extensions;
using OpenMLTD.MilliSim.Theater.Extensions;
using OpenMLTD.MilliSim.Theater.Internal;

namespace OpenMLTD.MilliSim.Theater.Elements {
    public class NotesLayer : BufferedElement2D {

        public NotesLayer(GameBase game)
            : base(game) {
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

        internal static readonly INoteTraceCalculator TraceCalculator = new RealisticNoteTraceCalculator();

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

            var gamingArea = theaterDays.FindSingleElement<GamingArea>();
            if (gamingArea == null) {
                throw new InvalidOperationException();
            }

            var settings = Program.Settings;
            var now = syncTimer.CurrentTime.TotalSeconds;
            var tapPointsLayout = settings.UI.TapPoints.Layout;
            var notesLayerLayout = settings.UI.NotesLayer.Layout;
            var clientSize = context.ClientSize;


            var opacity = settings.UI.NotesLayer.Opacity;

            var animationMetrics = new NoteAnimationMetrics {
                ClientSize = clientSize,
                GlobalSpeedScale = GlobalSpeedScale,
                Top = notesLayerLayout.Y * clientSize.Height,
                Bottom = tapPointsLayout.Y * clientSize.Height,
                NoteStartXRatios = tapPoints.IncomingXRatios,
                NoteEndXRatios = tapPoints.TapPointXRatios,
                TrackCount = tapPoints.TapPointXRatios.Length
            };

            var commonNoteMetrics = new NoteMetrics {
                StartRadius = gamingArea.ScaleResults.Note.Start,
                EndRadius = gamingArea.ScaleResults.Note.End,
                GlobalSpeedScale = GlobalSpeedScale
            };

            var specialNoteMetrics = new NoteMetrics {
                StartRadius = gamingArea.ScaleResults.SpecialNote.Start,
                EndRadius = gamingArea.ScaleResults.SpecialNote.End,
                GlobalSpeedScale = GlobalSpeedScale
            };

            context.Begin2D();

            foreach (var note in notes) {
                if (!NoteAnimationHelper.IsNoteVisible(note, now, commonNoteMetrics)) {
                    continue;
                }

                var imageIndex = -1;
                var isHugeNote = false;
                switch (note.Type) {
                    case RuntimeNoteType.Tap:
                        switch (note.Size) {
                            case RuntimeNoteSize.Small:
                                imageIndex = 0;
                                break;
                            case RuntimeNoteSize.Large:
                                imageIndex = 1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case RuntimeNoteType.Flick:
                        switch (note.FlickDirection) {
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
                    case RuntimeNoteType.Hold:
                        switch (note.FlickDirection) {
                            case FlickDirection.None:
                            case FlickDirection.Down:
                                switch (note.Size) {
                                    case RuntimeNoteSize.Small:
                                        imageIndex = 2;
                                        break;
                                    case RuntimeNoteSize.Large:
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
                    case RuntimeNoteType.Slide:
                        switch (note.FlickDirection) {
                            case FlickDirection.None:
                            case FlickDirection.Down:
                                if (note.IsSlideStart()) {
                                    imageIndex = 7;
                                } else if (note.IsSlideEnd()) {
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
                    case RuntimeNoteType.Special:
                        isHugeNote = true;
                        break;
                    case RuntimeNoteType.SpecialEnd:
                        // We don't draw this note.
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (imageIndex < 0 && !isHugeNote) {
                    continue;
                }

                if (!isHugeNote) {
                    var thisX = TraceCalculator.GetNoteX(note, now, commonNoteMetrics, animationMetrics);
                    var thisY = TraceCalculator.GetNoteY(note, now, commonNoteMetrics, animationMetrics);
                    var thisRadius = TraceCalculator.GetNoteRadius(note, now, commonNoteMetrics, animationMetrics);

                    if (settings.Style.SyncLine) {
                        // NextSync is always after PrevSync. So draw here, below the two notes.
                        if (note.HasNextSync()) {
                            var drawSyncLine = true;
                            if (note.IsSlideMiddle() || note.NextSync.IsSlideMiddle()) {
                                drawSyncLine = settings.Style.SlideMiddleSyncLine;
                            }
                            if (drawSyncLine) {
                                var nextX = TraceCalculator.GetNoteX(note.NextSync, now, commonNoteMetrics, animationMetrics);
                                context.DrawLine(_simpleSyncLinePen, thisX, thisY, nextX, thisY);
                            }
                        }
                    }

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

            var settings = Program.Settings;
            var theaterDays = Game.AsTheaterDays();
            var debugOverlay = theaterDays.FindSingleElement<DebugOverlay>();

            var gamingArea = theaterDays.FindSingleElement<GamingArea>();
            if (gamingArea == null) {
                throw new InvalidOperationException();
            }

            if (settings.Images.Notes == null || settings.Images.Notes.Length == 0) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine("WARNING: default notes image strip is not specified.");
                }
            } else {
                _noteImages = new D2DImageStrip[settings.Images.Notes.Length];

                for (var i = 0; i < settings.Images.Notes.Length; ++i) {
                    var notesImageEntry = settings.Images.Notes[i];
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

                    var imageStrip = Direct2DHelper.LoadImageStrip(context, notesImageEntry.File, notesImageEntry.Count, notesImageEntry.Orientation);
                    _noteImages[i] = imageStrip;
                }
            }

            if (settings.Images.SpecialNote.FileName == null) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine("WARNING: huge note image is not specified.");
                }
            } else if (!File.Exists(settings.Images.SpecialNote.FileName)) {
                if (debugOverlay != null) {
                    debugOverlay.AddLine($"WARNING: huge note image file <{settings.Images.SpecialNote.FileName}> is not found.");
                }
            } else {
                _specialNoteImage = Direct2DHelper.LoadBitmap(context, settings.Images.SpecialNote.FileName);
            }

            _simpleSyncLinePen = new D2DPen(context, Color.White, gamingArea.ScaleResults.SyncLine.Width);
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
        }

        [CanBeNull]
        private D2DBitmap _specialNoteImage;
        [ItemCanBeNull]
        private D2DImageStrip[] _noteImages;

        [CanBeNull]
        private RuntimeScore _score;

        private D2DPen _simpleSyncLinePen;

        private float _globalSpeedScale = 1f;

    }
}
