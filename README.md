# Percuro

## Übersicht

Ein datengetriebenes System zur Verwaltung und Organisation sensibler sowie öffentlicher Informationen. Ziel ist die Schaffung einer sicheren, benutzerfreundlichen und modularen Anwendung für Desktop Computer, mit einer Lite Version für Web/Mobile.

Als Example arbeiten wir mit den Daten eines imaginären Maschinenbau-Unternehmens

---
<h3>Aktueller Entwicklungsschritt</h3>  
Lagerbestandsverwaltung (InventoryView)
Derzeit arbeite ich an der Implementierung der InventoryView-Seite, die dazu dient, die Lagerbestände in Echtzeit darzustellen. Diese View zieht ihre Daten aus den Tabellen Artikel und Lagerorte und bietet eine detaillierte Übersicht der Bestände für jedes Lager und jeden Artikel.
Wichtige Funktionen, die gerade entwickelt werden: </br> </br>

~~Filter- und Suchfunktionen für eine gezielte Bestandsanzeige. </br>~~

Sortieroptionen nach Menge, Artikelname und Lagerort. </br>

Direktaktionen wie Bestandkorrekturen und Umbuchungen zwischen Lagerorten. </br>

Visualisierung von Bestandsmengen mit Warnungen bei niedrigem Bestand. </br>

Verknüpfung zu Artikeldetails für detaillierte Infos zu jedem Artikel. </br>

Der nächste Schritt wird die Verfeinerung der Filteroptionen und die Einführung von Reporting-Funktionen (z. B. CSV-Export) sein. </br>

---

## 📌 Phase 1: Planung und Konzeptualisierung

### 🎯 Ziele des Systems

- **Datenmanagement und -organisation**
  - Public / Private / Secret-Datenstruktur
- **Trennung von internen und öffentlichen Daten**
- **Sicherheit und Datenschutz**
- **Benutzerfreundlichkeit**
  - Dashboard, Rollen- und Rechteverwaltung

### ⚙️ Wichtige Funktionen

- **Datenbankstruktur**
  - MongoDB, SQL oder alternative Lösung
- **Benutzerverwaltung**
  - Rollen: Admin, Führungskräfte, Mitarbeiter
- **Rechteverwaltung**
  - Zugriffskontrolle auf Datenebene
- **Mehrsprachigkeit** *(optional)*
- **Integration mit externen Systemen**
  - E-Mail-Kommunikation, Webhooks, etc.

### 🧰 Technologiestack

- **Backend:**  
  - C# (Desktop)  
  - API für Web- & Mobile-Version
- **Frontend:**  
  - Next.js (Web)  
  - React (Mobile Web)
- **Datenbank:**  
  - MongoDB oder SQL
- **Security:**  
  - JWT / OAuth  
  - Verschlüsselung sensibler Daten

### 🎨 Design und UI

- Wireframes für Web & Desktop
- Benutzerführung für Admins und Endnutzer
- Mobile-First Design *(optional)*

![Percurio drawio](https://github.com/user-attachments/assets/e89d9743-8523-4a6a-90f8-a3ae88602c3d)

---

## 🖥️ Phase 2: Entwicklung des Desktop-Systems

### 🔧 Grundgerüst

- Einrichtung der Projektstruktur in C#
- UI-Layout für Dashboard & Datenverwaltung:

Dies ist ein Grundgerüst für die EnterpriseView die als Knotenpunkt für die ERP Spezifischen Bereiche dient.
![Enterprise drawio](https://github.com/user-attachments/assets/fedf9129-2e60-4fa6-8af8-0b9b79493760)

### 🔗 Datenbankanbindung

- Verbindung zu MongoDB oder SQL
- Logik zur Trennung privater und öffentlicher Daten
<h2>ERP-Datenmatrix (Maschinenbau-Unternehmen)</h2>

<h3>Stammdaten</h3>
<table>
  <thead>
    <tr>
      <th>Objekt</th>
      <th>Beschreibung</th>
      <th>Typ</th>
      <th>Abhängigkeiten</th>
    </tr>
  </thead>
  <tbody>
    <tr><td>Kunden</td><td>Firmenname, Adresse, Ansprechpartner, Branche, USt-ID</td><td>Stammdaten</td><td>Angebote, Aufträge, Rechnungen, Projekte</td></tr>
    <tr><td>Lieferanten</td><td>Firma, Adresse, Zahlungsbedingungen</td><td>Stammdaten</td><td>Bestellungen, Wareneingänge</td></tr>
    <tr><td>Artikel</td><td>Maschinen, Baugruppen, Teile (SKU, Preise, Einheit)</td><td>Stammdaten</td><td>Lager, Bestellungen, Fertigung, Rechnungen</td></tr>
    <tr><td>Mitarbeiter</td><td>Name, Personalnummer, Abteilung, Rolle</td><td>Stammdaten</td><td>Zeiterfassung, Projekte, Benutzerrechte</td></tr>
    <tr><td>Lagerorte</td><td>Standort, Regale, Kapazitäten</td><td>Stammdaten</td><td>Lagerbewegungen, Artikel</td></tr>
    <tr><td>Maschinen / Produkte</td><td>Verkaufte Maschinen (Seriennummer, Varianten)</td><td>Stammdaten</td><td>Kundenaufträge, Projekte, Wartung</td></tr>
    <tr><td>Projekte / Aufträge</td><td>Individuelle Kundenprojekte</td><td>Stammdaten</td><td>Zeiterfassung, Fertigung, Kunden</td></tr>
    <tr><td>Bankkonten</td><td>Firmenkonten für Zahlungen</td><td>Stammdaten</td><td>Zahlungen, Rechnungen</td></tr>
    <tr><td>Benutzer / Rollen</td><td>Logindaten, Rollenrechte</td><td>Stammdaten</td><td>Zugriffssteuerung</td></tr>
  </tbody>
</table>

<h3>Bewegungsdaten</h3>
<table>
  <thead>
    <tr>
      <th>Objekt</th>
      <th>Beschreibung</th>
      <th>Typ</th>
      <th>Abhängigkeiten</th>
    </tr>
  </thead>
  <tbody>
    <tr><td>Angebote</td><td>Preisanfragen an Kunden</td><td>Bewegungsdaten</td><td>Kunden, Artikel, Mitarbeiter</td></tr>
    <tr><td>Aufträge (Sales)</td><td>Beauftragte Leistungen / Produkte</td><td>Bewegungsdaten</td><td>Kunden, Projekte, Rechnungen</td></tr>
    <tr><td>Bestellungen (Einkauf)</td><td>Materialbeschaffung bei Lieferanten</td><td>Bewegungsdaten</td><td>Lieferanten, Artikel</td></tr>
    <tr><td>Wareneingänge</td><td>Erhalt & Prüfung bestellter Waren</td><td>Bewegungsdaten</td><td>Bestellungen, Artikel</td></tr>
    <tr><td>Lagerbewegungen</td><td>Einlagerung, Umbuchung, Verbrauch</td><td>Bewegungsdaten</td><td>Artikel, Lagerorte</td></tr>
    <tr><td>Fertigungsvorgänge</td><td>Aufträge an Produktion, Stücklisten</td><td>Bewegungsdaten</td><td>Artikel, Projekte, Mitarbeiter</td></tr>
    <tr><td>Rechnungen (Ausgang)</td><td>Abrechnung an Kunden</td><td>Bewegungsdaten</td><td>Kunden, Aufträge, Artikel, Bankkonto</td></tr>
    <tr><td>Rechnungen (Eingang)</td><td>Lieferantenrechnungen</td><td>Bewegungsdaten</td><td>Lieferanten, Bestellungen</td></tr>
    <tr><td>Zahlungen</td><td>Buchungen zu Rechnungen</td><td>Bewegungsdaten</td><td>Rechnungen, Bankkonten</td></tr>
    <tr><td>Zeiterfassung</td><td>Arbeitszeiten je Projekt</td><td>Bewegungsdaten</td><td>Mitarbeiter, Projekte</td></tr>
    <tr><td>Wartungshistorie</td><td>Service-Einsätze an Maschinen</td><td>Bewegungsdaten</td><td>Kunden, Maschinen, Mitarbeiter</td></tr>
  </tbody>
</table>
### 🔐 Sicherheit

- Login-System mit Benutzerrollen
- Verschlüsselung vertraulicher Daten
- Backup- und Wiederherstellungsstrategien

### 🧩 Funktionalitäten

- E-Mail-Integration für Benachrichtigungen
- Daten-Import/-Export
- Intensive Tests und Fehlerbehebung

---

## 🌐 Phase 3: Entwicklung der Web- und Mobile-Version

### 🌍 Backend API

- RESTful API zur Kommunikation mit allen Clients
- Authentifizierte Routen je nach Benutzerrolle

### 💻 Web-Version (Next.js)

- UI für Benutzer-Dashboard und Datenanzeige
- Formulare, Buttons, Benutzerinteraktionen
- Leichtgewichtige und performante Umsetzung

### 📱 Mobile Web-Version

- Mobile-First Design
- Reduzierter Funktionsumfang (nur öffentliche Daten)

---

## 🧪 Phase 4: Tests und Qualitätssicherung

### ✅ Funktionale Tests

- Desktop, Web und Mobile-Version

### 🔒 Sicherheitstests

- Penetrationstests (z. B. gegen XSS, SQL-Injection)
- Datenintegritäts-Checks

### 👥 Benutzerakzeptanztest (UAT)

- Feedback von realen Testnutzern einholen und evaluieren


## 🚀 Phase 5: Veröffentlichung und Wartung

### 🗃️ Veröffentlichung der Desktop-App

- Erstellung eines Installationspakets
- Optionale Bereitstellung über App Stores

### 🌐 Veröffentlichung der Web-Version

- Hosting über Plattformen wie Vercel oder Netlify

### 🔄 Wartung und Updates

- Regelmäßige Funktions- und Sicherheitsupdates
- Zeitnahe Behebung von Bugs

---

## 🔮 Langfristige Perspektive

- **Modul-Erweiterung:** z. B. E-Commerce, CRM
- **Cloud-Integration:** hybride Speicherung lokal + Cloud *(zukünftig)*

---
