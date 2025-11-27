namespace BlueDotBrigade.Analyzers.Dsl;

/// <summary>
/// Shared defaults for DSL configuration content that can be reused by analyzers
/// and command-line tooling.
/// </summary>
public static class DslDefaults
{
    public const string DefaultDslFileName = "dsl.config.xml";

    public const string DefaultDslXml = """
<dsl>
  <term prefer="Customer" block="Client" case="sensitive"/>
  <term prefer="Customer" case="sensitive">
    <alias block="Client"/>
    <alias block="Cust"/>
  </term>
</dsl>
""";
}
