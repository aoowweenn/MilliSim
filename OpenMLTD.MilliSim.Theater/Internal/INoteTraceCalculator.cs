using System.Drawing;
using JetBrains.Annotations;
using OpenMLTD.MilliSim.Core.Entities.Runtime;

namespace OpenMLTD.MilliSim.Theater.Internal {
    /// <summary>
    /// An interface used to calculate traces of notes.
    /// </summary>
    internal interface INoteTraceCalculator {

        /// <summary>
        /// Calculate selected note's radius according to given data.
        /// </summary>
        /// <param name="note">The note whose current radius is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>Calculated note radius.</returns>
        SizeF GetNoteRadius([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        /// <summary>
        /// Calculate selected note's X position according to given data.
        /// </summary>
        /// <param name="note">The note whose current X position is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>The note's X position.</returns>
        float GetNoteX([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        /// <summary>
        /// Calculate selected note's Y position according to given data.
        /// </summary>
        /// <param name="note">The note whose current Y position is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>The note's Y position.</returns>
        float GetNoteY([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        /// <summary>
        /// Calculate special note's radius according to given data.
        /// </summary>
        /// <param name="note">The note whose current radius is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>Calculated note radius.</returns>
        SizeF GetSpecialNoteRadius([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        /// <summary>
        /// Calculate special note's X position according to given data.
        /// </summary>
        /// <param name="note">The special note whose current X position is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>The note's X position.</returns>
        float GetSpecialNoteX([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        /// <summary>
        /// Calculate special note's Y position according to given data.
        /// </summary>
        /// <param name="note">The special note whose current Y position is to be calculated.</param>
        /// <param name="now">Current time, in seconds.</param>
        /// <param name="noteMetrics">Note metrics.</param>
        /// <param name="animationMetrics">Note animation metrics.</param>
        /// <returns>The note's Y position.</returns>
        float GetSpecialNoteY([NotNull] RuntimeNote note, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

        (float X1, float Y1, float ControlX1, float ControlY1, float ControlX2, float ControlY2, float X2, float Y2) GetRibbonLocations([NotNull] RuntimeNote thisNote, [NotNull] RuntimeNote nextNote, double now, NoteMetrics noteMetrics, NoteAnimationMetrics animationMetrics);

    }
}
