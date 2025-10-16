namespace Winzer.Library;

public class TestContext
{
    public static readonly TestContext Live = new TestContext { Dryrun = false };

    /// <summary>
    /// Run the job without modifying anything in Shopify.
    /// </summary>
    public bool Dryrun { get; set; } = true;
    /// <summary>
    /// Generate export files for testing when doing a Dryrun.
    /// </summary>
    public bool Test { get; set; } = false;
}
