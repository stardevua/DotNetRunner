using System.Collections.Generic;

namespace ProjectRunner.Models
{
    public class SolutionConfig
    {
        public IList<LaunchProfileConfig> LaunchProfiles { get; set; }
    }

    public class LaunchProfileConfig
    {
        public string ProjectName { get; set; }
        public string ProjectFullName { get; set; }
        public string ProfileName { get; set; }

        public string DisplayText => $"{ProjectName} - {ProfileName}";
    }
}
