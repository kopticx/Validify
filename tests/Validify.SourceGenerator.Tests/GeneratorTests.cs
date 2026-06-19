namespace Validify.SourceGenerator.Tests;

public class GeneratorTests
{
  [Test]
  public Task EmptyCompilation_GeneratesEmptyRegistrationMethod()
  {
    const string source = "namespace Sample { public class Nothing { } }";

    var driver = TestHelper.Run(source);

    return Verify(driver);
  }
}