namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Wraps members in a consumer file that imports DotMaybe the documented way.
/// </summary>
internal static class Consumer
{
    public static string File(string members, string inNamespace = "Consumer") => $$"""
          using System;
          using System.Threading.Tasks;
          using DotMaybe;
          using static DotMaybe.Prelude;

          namespace {{inNamespace}};

          public static class Sample
          {
          {{members}}
          }
          """;
}
