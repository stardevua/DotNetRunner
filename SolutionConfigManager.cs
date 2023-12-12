using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ProjectRunner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectRunner
{
    internal class SolutionConfigManager
    {
        public static SolutionConfig InitializeConfig(string configFilePath, Projects projects)
        {

            var profiles = GetLaunchProfiles(projects);

            if (!File.Exists(configFilePath))
            {
                var config = new SolutionConfig();
                config.LaunchProfiles = profiles;
                WriteConfig(config, configFilePath);

                return config;
            }

            string json = File.ReadAllText(configFilePath);
            var configFromFile = JsonSerializer.Deserialize<SolutionConfig>(json);
            return configFromFile;
        }

        private static List<LaunchProfileConfig> GetLaunchProfiles(Projects projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<LaunchProfileConfig> profiles = new List<LaunchProfileConfig>();
            foreach (Project project in projects)
            {
                string projectPath = Path.GetDirectoryName(project.FullName);
                string launchSettingsPath = Path.Combine(projectPath, "Properties", "launchSettings.json");

                if (!File.Exists(launchSettingsPath))
                {
                    continue;
                }    
                string jsonContent = File.ReadAllText(launchSettingsPath);
                dynamic launchSettings = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);

                foreach (var profile in launchSettings.profiles)
                {
                    if (profile.Value.commandName == "Project")
                    {
                        profiles.Add(new LaunchProfileConfig
                        {
                            ProjectName = project.Name,
                            ProfileName = profile.Name,
                            ProjectFullName = project.FullName,
                        });
                    }
                }
            }

            return profiles;
        }

        public static void WriteConfig(SolutionConfig config, string configFilePath)
        {
            var directory = Path.GetDirectoryName(configFilePath);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }


            string json = System.Text.Json.JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFilePath, json);
        }
    }
}
