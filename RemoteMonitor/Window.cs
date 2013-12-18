using System;

namespace RemoteMonitor
{
    public class Window
    {
        /// <summary>
        ///     Gibt an, Welcher Art das Fenster ist da je nach Typ verschiedene Regeln gelten
        /// </summary>
        public enum WinType
        {
            Normal = 0,
            Explorer = 1
        }


        /// <summary>
        ///     Erstellt eine neue Instanz der Klasse mit den angegebenen Paramrtern
        /// </summary>
        /// <param name="title">Der Titel des Fensters</param>
        /// <param name="handle">Das Fensterhandle des Fensters</param>
        /// <param name="classe">Die Klasse/Art des Fensters</param>
        /// <param name="isVisible">True wenn der Fenster minimiert ist</param>
        /// <param name="pos">Die Position des Fensters auf dem Bildschirm</param>
        /// <param name="size">Die Größe des Fensters</param>
        public Window(string title, IntPtr handle, string classe, bool isVisible,
            Declarations.Point pos, Declarations.Point size, WinType type)
        {
            winTitle = title;
            winHandle = handle;
            winClass = classe;
            winVisible = isVisible;
            winPos = pos;
            winSize = size;
            winType = type;
        }

        public string winTitle { get; set; } //Titel des Fensters
        public IntPtr winHandle { get; set; } //Das Handle des Fensters
        public bool winVisible { get; set; } //Gibt an, ob das Fenster sichtbar ist
        public Declarations.Point winSize { get; set; } //Größe des Fenster
        public Declarations.Point winPos { get; set; } //Position des Fensters
        public string winClass { get; set; } //Klasse des Fensters
        public WinType winType { get; set; } //Bitte ignorieren, spielt für die Demo keine Rolle
    }
}