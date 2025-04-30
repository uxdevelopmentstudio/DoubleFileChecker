# DoubleFileChecker

Eine WPF-Beispiel-Anwendung zur Erkennung und Verwaltung von Dateiduplikaten mit erweiterten Visualisierungsfunktionen.

## Kernfunktionen
- 🔍 **Duplikaterkennung** mittels SHA256-Hashvergleich
- 🖼️ **Bildvorschauen** für unterstützte Bildformate (JPG, PNG, BMP)
  - **Hover-Informationen**: Zeigt Dateipfade bei Mouse-Hover über Bildern
- ⚡ **Automatische Selektion** von Duplikatgruppen (>1 Vorkommen)
- 🗑️ **Sicheres Löschen** markierter Duplikate mit Bestätigungsdialog
- 📁 **Rekursive Ordneranalyse** mit konfigurierbaren Suchoptionen

## Technische Highlights
- 🧩 **MVVM-Architektur** mit klarer Trennung von Logik und UI
- 📊 **Performance-optimierte Datenverarbeitung**:
  - Virtulisierter DataGrid für große Datensätze
- 🛠️ **Erweiterte Filteroptionen**:
  - Bilddateine (JPG, PNG, BMP)
