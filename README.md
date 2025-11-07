# BlueDotBrigade.Analyzers

**BlueDotBrigade.Analyzers** is a custom Roslyn analyzer that enforces domain-specific naming consistency in C# codebases using a simple XML-based Domain-Specific Language (DSL).

---

## 🧩 Overview

The analyzer scans identifiers (types, methods, fields, properties, and variables) for disallowed terms defined in a DSL configuration file and reports compile-time diagnostics when blocked words appear.

If the DSL file isn’t found, a default example configuration is generated automatically.

---

## ⚙️ Example DSL Configuration

```xml
<dsl>
  <!-- One-off -->
  <term prefer="Customer" block="Client" case="sensitive"/>

  <!-- One-to-many -->
  <term prefer="Customer" case="sensitive">
    <alias block="Client"/>
    <alias block="Cust"/>
  </term>
</dsl>
```

### Rules

* **`prefer`** — the preferred or canonical term
* **`block`** — a forbidden or deprecated term
* **`case`** — optional; defaults to `sensitive` (`insensitive` also supported)
* **`alias`** — nested element for multiple blocked terms

---

## 🧱 Installation

Add the NuGet package to any project or solution:

```bash
dotnet add package BlueDotBrigade.Analyzers
```

---

## 📁 Configuration

Place your DSL file (e.g., `dsl.config.xml`) in the solution root or project directory.

Then, expose it as an **AdditionalFile** in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <AnalyzerDslFileName>dsl.config.xml</AnalyzerDslFileName>
  </PropertyGroup>

  <ItemGroup>
    <CompilerVisibleProperty Include="AnalyzerDslFileName" />
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)$(AnalyzerDslFileName)" />
  </ItemGroup>
</Project>
```

If no DSL file is found, the analyzer uses a built-in default example.

---

## 🥪 Testing

Unit tests use **MSTest v3+** and **BlueDotBrigade.DatenLokator** for sample input management.

Run all tests with:

```bash
dotnet test
```

---

## 🔍 Requirements

* .NET SDK **9.0+**
* Visual Studio 2022 (v17.10+) or newer
* Compatible with Roslyn-based tooling (e.g., JetBrains Rider)
