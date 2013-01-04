using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SharePointAuthenticationSkeleton.Common
{
    /// <summary>
    /// Typische Implementierung von Page, die mehrere wichtige Vorteile bietet:
    /// <list type="bullet">
    /// <item>
    /// <description>Zuordnung des Ansichtszustands der Anwendung zum visuellen Zustand</description>
    /// </item>
    /// <item>
    /// <description>GoBack-, GoForward- und GoHome-Ereignishandler</description>
    /// </item>
    /// <item>
    /// <description>Maus- und Tastenkombinationen für die Navigation</description>
    /// </item>
    /// <item>
    /// <description>Zustandsverwaltung für Navigation und Verwaltung der Prozesslebensdauer</description>
    /// </item>
    /// <item>
    /// <description>Ein Standardanzeigemodell</description>
    /// </item>
    /// </list>
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class LayoutAwarePage : Page
    {
        /// <summary>
        /// Identifiziert die <see cref="DefaultViewModel"/>-Abhängigkeitseigenschaft.
        /// </summary>
        public static readonly DependencyProperty DefaultViewModelProperty =
            DependencyProperty.Register("DefaultViewModel", typeof(IObservableMap<String, Object>),
            typeof(LayoutAwarePage), null);

        private List<Control> _layoutAwareControls;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="LayoutAwarePage"/>-Klasse.
        /// </summary>
        public LayoutAwarePage()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            // Ein leeres Standardanzeigemodell erstellen
            this.DefaultViewModel = new ObservableDictionary<String, Object>();

            // Zwei Änderungen vornehmen, wenn diese Seite Teil der visuellen Struktur ist:
            // 1) Den Ansichtszustand der Anwendung dem visuellen Zustand für die Seite zuordnen
            // 2) Tastatur- und Mausnavigationsanforderungen bearbeiten
            this.Loaded += (sender, e) =>
            {
                this.StartLayoutUpdates(sender, e);

                // Tastatur- und Mausnavigation trifft nur zu, wenn das gesamte Fenster ausgefüllt wird.
                if (this.ActualHeight == Window.Current.Bounds.Height &&
                    this.ActualWidth == Window.Current.Bounds.Width)
                {
                    // Das Fenster direkt überwachen, sodass kein Fokus erforderlich ist
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        this.CoreWindow_PointerPressed;
                }
            };

            // Dieselben Änderungen rückgängig machen, wenn die Seite nicht mehr sichtbar ist
            this.Unloaded += (sender, e) =>
            {
                this.StopLayoutUpdates(sender, e);
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    this.CoreWindow_PointerPressed;
            };
        }

        /// <summary>
        /// Eine Implementierung von <see cref="IObservableMap&lt;String, Object&gt;"/>, die als
        /// triviales Anzeigemodell verwendet werden kann.
        /// </summary>
        protected IObservableMap<String, Object> DefaultViewModel
        {
            get
            {
                return this.GetValue(DefaultViewModelProperty) as IObservableMap<String, Object>;
            }

            set
            {
                this.SetValue(DefaultViewModelProperty, value);
            }
        }

        #region Navigationsunterstützung dient.

        /// <summary>
        /// Wird aufgerufen als Ereignishandler, um rückwärts im mit der Seite verknüpften
        /// <see cref="Frame"/> zu navigieren, bis der Anfang des Navigationsstapels erreicht wird.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="e">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt haben.</param>
        protected virtual void GoHome(object sender, RoutedEventArgs e)
        {
            // Den Navigationsframe zum Zurückkehren zur obersten Seite verwenden
            if (this.Frame != null)
            {
                while (this.Frame.CanGoBack) this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Wird aufgerufen als Ereignishandler, um rückwärts im Navigationsstapel zu navigieren,
        /// der mit dem <see cref="Frame"/> dieser Seite verknüpft ist.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="e">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt
        /// haben.</param>
        protected virtual void GoBack(object sender, RoutedEventArgs e)
        {
            // Den Navigationsframe zum Zurückkehren zur vorherigen Seite verwenden
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        /// <summary>
        /// Wird aufgerufen als Ereignishandler, um vorwärts im Navigationsstapelereignis
        /// der mit dem <see cref="Frame"/> dieser Seite verknüpft ist.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="e">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt
        /// haben.</param>
        protected virtual void GoForward(object sender, RoutedEventArgs e)
        {
            // Den Navigationsframe zum Wechseln zur nächsten Seite verwenden
            if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
        }

        /// <summary>
        /// Wird bei jeder Tastatureingabe aufgerufen, einschließlich Systemtasten wie ALT-Tastenkombinationen, wenn
        /// diese Seite aktiv ist und das gesamte Fenster ausfüllt. Wird verwendet zum Erkennen von Tastaturnavigation
        /// zwischen Seiten, auch wenn sich die Seite selbst nicht im Fokus befindet.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="args">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt haben.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs args)
        {
            var virtualKey = args.VirtualKey;

            // Weitere Untersuchungen nur durchführen, wenn die Taste "Nach links", "Nach rechts" oder die dezidierten Tasten "Zurück" oder "Weiter"
            // gedrückt werden
            if ((args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                args.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // Wenn die Taste "Zurück" oder ALT+NACH-LINKS-TASTE gedrückt wird, zurück navigieren
                    args.Handled = true;
                    this.GoBack(this, new RoutedEventArgs());
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // Wenn die Taste "Weiter" oder ALT+NACH-RECHTS-TASTE gedrückt wird, vorwärts navigieren
                    args.Handled = true;
                    this.GoForward(this, new RoutedEventArgs());
                }
            }
        }

        /// <summary>
        /// Wird bei jedem Mausklick, jeder Touchscreenberührung oder einer äquivalenten Interaktion aufgerufen, wenn diese
        /// Seite aktiv ist und das gesamte Fenster ausfüllt. Wird zum Erkennen von "Weiter"- und "Zurück"-Maustastenklicks
        /// im Browserstil verwendet, um zwischen Seiten zu navigieren.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="args">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt haben.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            // Tastenkombinationen mit der linken, rechten und mittleren Taste ignorieren
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed) return;

            // Wenn "Zurück" oder "Vorwärts" gedrückt wird (jedoch nicht gleichzeitig), entsprechend navigieren
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                args.Handled = true;
                if (backPressed) this.GoBack(this, new RoutedEventArgs());
                if (forwardPressed) this.GoForward(this, new RoutedEventArgs());
            }
        }

        #endregion

        #region Wechseln zwischen visuellen Zuständen

        /// <summary>
        /// Wird aufgerufen als Ereignishandler, normalerweise für das <see cref="FrameworkElement.Loaded"/>-Ereignis
        /// von einem <see cref="Control"/> innerhalb der Seite, um anzugeben, dass der Absender mit dem
        /// Empfang von Änderungen der visuellen Zustandsverwaltung beginnen soll, die Änderungen am Ansichtszustand der Anwendung
        /// entsprechen.
        /// </summary>
        /// <param name="sender">Instanz von <see cref="Control"/>, die die Verwaltung von visuellen Zuständen
        /// entsprechend den Ansichtszuständen unterstützt.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie die Anforderung durchgeführt wurde.</param>
        /// <remarks>Der aktuelle Ansichtszustand wird sofort verwendet, um den entsprechenden
        /// visuellen Zustand festzulegen, wenn Layoutaktualisierungen angefordert werden. Ein entsprechender
        /// <see cref="FrameworkElement.Unloaded"/>-Ereignishandler der mit
        /// <see cref="StopLayoutUpdates"/> verbunden ist, wird dringend empfohlen. Instanzen von
        /// <see cref="LayoutAwarePage"/> rufen diese Handler in den zugehörigen Loaded- und
        /// Unloaded-Ereignissen automatisch auf.</remarks>
        /// <seealso cref="DetermineVisualState"/>
        /// <seealso cref="InvalidateVisualState"/>
        public void StartLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;
            if (this._layoutAwareControls == null)
            {
                // Überwachung der Änderungen am Ansichtszustand starten, wenn an Aktualisierungen interessierte Steuerelemente vorliegen
                Window.Current.SizeChanged += this.WindowSizeChanged;
                this._layoutAwareControls = new List<Control>();
            }
            this._layoutAwareControls.Add(control);

            // Den anfänglichen visuellen Zustand des Steuerelements festlegen
            VisualStateManager.GoToState(control, DetermineVisualState(ApplicationView.Value), false);
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// Wird aufgerufen als Ereignishandler, normalerweise für das <see cref="FrameworkElement.Unloaded"/>-Ereignis
        /// eines <see cref="Control"/>, um anzugeben, dass der Absender mit dem Empfang
        /// Änderungen der visuellen Zustandsverwaltung beginnen soll, die Änderungen am Ansichtszustand der Anwendung entsprechen.
        /// </summary>
        /// <param name="sender">Instanz von <see cref="Control"/>, die die Verwaltung von visuellen Zuständen
        /// entsprechend den Ansichtszuständen unterstützt.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie die Anforderung durchgeführt wurde.</param>
        /// <remarks>Der aktuelle Ansichtszustand wird sofort verwendet, um den entsprechenden
        /// des visuellen Zustands beginnen soll, wenn Layoutaktualisierungen angefordert werden.</remarks>
        /// <seealso cref="StartLayoutUpdates"/>
        public void StopLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null || this._layoutAwareControls == null) return;
            this._layoutAwareControls.Remove(control);
            if (this._layoutAwareControls.Count == 0)
            {
                // Überwachung der Änderungen am Ansichtszustand beenden, wenn keine Steuerelemente an Aktualisierungen interessiert sind
                this._layoutAwareControls = null;
                Window.Current.SizeChanged -= this.WindowSizeChanged;
            }
        }

        /// <summary>
        /// Übersetzt <see cref="ApplicationViewState"/>-Werte in Zeichenfolgen für die visuelle Zustandsverwaltung
        /// innerhalb der Seite. Die Standardimplementierung verwendet die Namen von Enumerationswerten.
        /// Unterklassen können diese Methode überschreiben, um das verwendete Zuordnungsschema zu steuern.
        /// </summary>
        /// <param name="viewState">Ansichtszustand, für den ein visueller Zustand gewünscht wird.</param>
        /// <returns>Name des visuellen Zustands zur Verwendung mit dem
        /// <see cref="VisualStateManager"/></returns>
        /// <seealso cref="InvalidateVisualState"/>
        protected virtual string DetermineVisualState(ApplicationViewState viewState)
        {
            return viewState.ToString();
        }

        /// <summary>
        /// Aktualisiert alle Steuerelemente, die auf Änderungen des visuellen Zustands lauschen, mit dem korrekten visuellen
        /// Zustand.
        /// </summary>
        /// <remarks>
        /// Wird normalerweise in Verbindung mit dem Überschreiben von <see cref="DetermineVisualState"/> verwendet, um
        /// signalisieren, dass ein anderer Wert zurückgegeben kann, obwohl der Ansichtszustand nicht
        /// geändert wurde.
        /// </remarks>
        public void InvalidateVisualState()
        {
            if (this._layoutAwareControls != null)
            {
                string visualState = DetermineVisualState(ApplicationView.Value);
                foreach (var layoutAwareControl in this._layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, visualState, false);
                }
            }
        }

        #endregion

        #region Verwaltung der Prozesslebensdauer

        private String _pageKey;

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Rahmen angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde. Die
        /// Parametereigenschaft stellt die anzuzeigende Gruppe bereit.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Das Laden des Zustands sollte beim Zurückspringen zu einer zwischengespeicherten Seite über die Navigation nicht ausgelöst werden.
            if (this._pageKey != null) return;

            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Vorhandenen Zustand für die Vorwärtsnavigation löschen, wenn dem Navigationsstapel eine neue
                // Seite hinzugefügt wird
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // Den Navigationsparameter an die neue Seite übergeben
                this.LoadState(e.Parameter, null);
            }
            else
            {
                // Den Navigationsparameter und den beibehaltenen Seitenzustand an die Seite übergeben,
                // dabei die gleiche Strategie verwenden wie zum Laden des angehaltenen Zustands und zum erneuten Erstellen von im Cache verworfenen
                // Seiten
                this.LoadState(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]);
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite nicht mehr in einem Rahmen angezeigt wird.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde. Die
        /// Parametereigenschaft stellt die anzuzeigende Gruppe bereit.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<String, Object>();
            this.SaveState(pageState);
            frameState[_pageKey] = pageState;
        }

        /// <summary>
        /// Füllt die Seite mit Inhalt auf, der bei der Navigation übergeben wird. Gespeicherte Zustände werden ebenfalls
        /// bereitgestellt, wenn eine Seite aus einer vorherigen Sitzung neu erstellt wird.
        /// </summary>
        /// <param name="navigationParameter">Der Parameterwert, der an
        /// <see cref="Frame.Navigate(Type, Object)"/> übergeben wurde, als diese Seite ursprünglich angefordert wurde.
        /// </param>
        /// <param name="pageState">Ein Wörterbuch des Zustands, der von dieser Seite während einer früheren Sitzung
        /// beibehalten wurde. Beim ersten Aufrufen einer Seite ist dieser Wert NULL.</param>
        protected virtual void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Behält den dieser Seite zugeordneten Zustand bei, wenn die Anwendung angehalten oder
        /// die Seite im Navigationscache verworfen wird. Die Werte müssen den Serialisierungsanforderungen
        /// von <see cref="SuspensionManager.SessionState"/> entsprechen.
        /// </summary>
        /// <param name="pageState">Ein leeres Wörterbuch, das mit dem serialisierbaren Zustand aufgefüllt wird.</param>
        protected virtual void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #endregion

        /// <summary>
        /// Implementierung von IObservableMap, die ein erneutes Eintreten zur Verwendung als Standardanzeigemodell
        /// unterstützt.
        /// </summary>
        private class ObservableDictionary<K, V> : IObservableMap<K, V>
        {
            private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<K>
            {
                public ObservableDictionaryChangedEventArgs(CollectionChange change, K key)
                {
                    this.CollectionChange = change;
                    this.Key = key;
                }

                public CollectionChange CollectionChange { get; private set; }
                public K Key { get; private set; }
            }

            private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
            public event MapChangedEventHandler<K, V> MapChanged;

            private void InvokeMapChanged(CollectionChange change, K key)
            {
                var eventHandler = MapChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new ObservableDictionaryChangedEventArgs(change, key));
                }
            }

            public void Add(K key, V value)
            {
                this._dictionary.Add(key, value);
                this.InvokeMapChanged(CollectionChange.ItemInserted, key);
            }

            public void Add(KeyValuePair<K, V> item)
            {
                this.Add(item.Key, item.Value);
            }

            public bool Remove(K key)
            {
                if (this._dictionary.Remove(key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                    return true;
                }
                return false;
            }

            public bool Remove(KeyValuePair<K, V> item)
            {
                V currentValue;
                if (this._dictionary.TryGetValue(item.Key, out currentValue) &&
                    Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                    return true;
                }
                return false;
            }

            public V this[K key]
            {
                get
                {
                    return this._dictionary[key];
                }
                set
                {
                    this._dictionary[key] = value;
                    this.InvokeMapChanged(CollectionChange.ItemChanged, key);
                }
            }

            public void Clear()
            {
                var priorKeys = this._dictionary.Keys.ToArray();
                this._dictionary.Clear();
                foreach (var key in priorKeys)
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                }
            }

            public ICollection<K> Keys
            {
                get { return this._dictionary.Keys; }
            }

            public bool ContainsKey(K key)
            {
                return this._dictionary.ContainsKey(key);
            }

            public bool TryGetValue(K key, out V value)
            {
                return this._dictionary.TryGetValue(key, out value);
            }

            public ICollection<V> Values
            {
                get { return this._dictionary.Values; }
            }

            public bool Contains(KeyValuePair<K, V> item)
            {
                return this._dictionary.Contains(item);
            }

            public int Count
            {
                get { return this._dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
                int arraySize = array.Length;
                foreach (var pair in this._dictionary)
                {
                    if (arrayIndex >= arraySize) break;
                    array[arrayIndex++] = pair;
                }
            }
        }
    }
}
