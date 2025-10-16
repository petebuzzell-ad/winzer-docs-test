using System.ComponentModel.DataAnnotations;

namespace Winzer.Repo
{
    public class RepoOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string WinzerDBConnectionString { get; set; } = string.Empty;
    }
}
