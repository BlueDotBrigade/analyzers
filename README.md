# BlueDotBrigade.Analyzers

Enforces domain-specific naming consistency in C# codebases using a simple XML-based Domain-Specific Language (DSL).

---

## 🧩 Overview

The analyzer scans identifiers (types, methods, fields, properties, and variables) for disallowed terms defined in a DSL configuration file and reports compile-time diagnostics when blocked words appear.

If the DSL file isn’t found, the analyzer reports a configuration warning and runs with an empty rule set (no identifiers are flagged). No files are written to disk by the analyzer.

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

* `prefer` — the preferred or canonical term
* `block` — a forbidden or deprecated term
* `case` — optional; defaults to `sensitive` (`insensitive` also supported)
* `alias` — nested element for multiple blocked terms

---

## 🧱 Installation

Add the NuGet package to any project or solution:

```bash
dotnet add package BlueDotBrigade.Analyzers
```

---

## 📁 Configuration (solution-level and project-level)

The analyzer supports a shared DSL and per-project overrides. By default, the expected filename is `dsl.config.xml`. You can change it by setting the `AnalyzerDslFileName` property.

Precedence:
- Project-level DSL takes precedence over the solution-level DSL.

How it is wired (best practice for .NET10 analyzers):
- The analyzer package exposes two compiler-visible properties: `AnalyzerDslFileName` and `MSBuildProjectDirectory`.
- The analyzer project configures `AdditionalFiles` so the compiler sees:
 - `$(SolutionDir)$(AnalyzerDslFileName)` when it exists (shared/solution-level)
 - `$(MSBuildProjectDirectory)\$(AnalyzerDslFileName)` when it exists (project-level)
- The analyzer prefers the project-local file when both are present.

To opt into a custom filename, define in your solution’s `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <AnalyzerDslFileName>dsl.config.xml</AnalyzerDslFileName>
  </PropertyGroup>
</Project>
```

Place your files accordingly:
- Solution-level: `$(SolutionDir)/dsl.config.xml`
- Project-level: `$(ProjectDir)/dsl.config.xml`

If neither file exists, you’ll see warning `RC000` with a sample DSL, and no identifiers will be flagged.

---

## 🛠 Command-line helper

The repository includes a small console utility (`BlueDotBrigade.Analyzers.Tool`) that can scaffold the sample DSL configuration
file the analyzer expects.

Common usage examples:

```bash
# Print the XML to the console
dotnet run --project Src/BlueDotBrigade.Analyzers.Tool -- generate-dsl --stdout

# Write the XML to the default dsl.config.xml in the current directory
dotnet run --project Src/BlueDotBrigade.Analyzers.Tool -- generate-dsl

# Overwrite or place the DSL file in a custom location
dotnet run --project Src/BlueDotBrigade.Analyzers.Tool -- generate-dsl --output ./config/dsl.config.xml --force
```

Once packaged as a .NET tool you can invoke it as `bdb-analyzers generate-dsl` and pass the same options.

---

## 🥪 Testing

Unit tests use MSTest and `Microsoft.CodeAnalysis.Testing` harness.

- Tests provide `AdditionalFiles` directly to simulate solution-level and project-level configurations.
- The analyzer uses `MSBuildProjectDirectory` (exposed to the compiler) to prefer project-level when both are present.

Run all tests with:

```bash
dotnet test
```

---

## 🔍 Requirements

* .NET SDK10.0+
* Visual Studio2022 (v17.10+) or newer

---

## 🧯 Overriding or Suppressing Rules

Same approaches as standard Roslyn analyzers: `#pragma`, `[SuppressMessage]`, and `.editorconfig` severity configuration.
