using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteMonitor
{
    public class Windows
    {
        /// <summary>
        ///     Delegate Funktion für EnumWindows (Siehe Declarations)
        ///     Gibt die Werte an EnumWindowCallBack weiter
        /// </summary>
        /// <param name="hwnd">Fensterhandle</param>
        /// <param name="lParam">Nicht benutzt bitte 0 angeben</param>
        /// <returns>Bei Erfolg True</returns>
        public delegate bool WinCallBack(int hwnd, int lParam);

        /// <summary>
        ///     Erstellt eine neue Windows-Klasse und ließt die Informationen
        ///     über alle verfügbaren Fenster aus
        /// </summary>
        public Windows()
        {
            lstWindows = new LinkedList<Window>();

            getWindows();
        }

        /// <summary>
        ///     Beinhaltet alle zur Zeit geöffneten und Minimierten Fenster
        /// </summary>
        public LinkedList<Window> lstWindows { get; set; }

        private void getWindows()
        {
            //Liste mit Fenstern befüllen
            Declarations.EnumWindows(new WinCallBack(EnumWindowCallBack), 0);
        }

        /// <summary>
        ///     Gibt die Anzahl gefundener Fenster zurück
        /// </summary>
        /// <returns>Integer mit der Anahl der Fenster</returns>
        public int winCount()
        {
            return lstWindows.Count();
        }

        /// <summary>
        ///     Diese Funktion wird durch die Delegate Funktion WinCallBack aufgerufen
        ///     und iteriert durch alle zur Zeit geöffneten Fenster
        /// </summary>
        /// <param name="hwnd">Erhält den Fensterhandle</param>
        /// <param name="lParam">Diese Variable ist ungenutzt!</param>
        /// <returns>Bei Erfolg gibt diese Funktion True zurück</returns>
        private bool EnumWindowCallBack(int hwnd, int lParam)
        {
            var windowHandle = (IntPtr) hwnd;

            var sb = new StringBuilder(1024);
            var sbc = new StringBuilder(256);

            Declarations.GetClassName(hwnd, sbc, sbc.Capacity);
            Declarations.GetWindowText((int) windowHandle, sb, sb.Capacity);

            //Nur Prozesse mit einer Beschreibung, also einem Fenster bearbeiten
            if (sb.Length > 0)
            {
                var r = new Declarations.RECT(); //Fensterposition & Größe bestimmen:
                Declarations.GetWindowRect(windowHandle, ref r);


                var w = new Window(sb + "",
                    windowHandle,
                    sbc + "",
                    Declarations.IsWindowVisible(windowHandle),
                    new Declarations.Point(r.Left, r.Top),
                    new Declarations.Point(r.Right - r.Left, r.Bottom - r.Top),
                    Window.WinType.Normal);
                lstWindows.AddLast(w);
            }
            return true;
        }
    }
}