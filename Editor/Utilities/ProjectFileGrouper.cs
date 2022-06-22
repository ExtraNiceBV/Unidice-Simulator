using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Unidice.Simulator.Utilities
{
    /// <summary>
    /// When updating the solution file, group every project that does not contain an ignored string into a folder.
    /// </summary>
    public class ProjectFileGrouper : AssetPostprocessor
    {
        private const string FOLDER = "Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"External\", \"External\", \"{978CDBB4-A01F-40E1-A10F-837756A7234A}\"\r\nEndProject\r\n";
        private const string FOLDER_ID = "{978CDBB4-A01F-40E1-A10F-837756A7234A}";
        private const string FOLDER_NAME = "External";

        private const string IGNORED = "Unidice";


        public static string OnGeneratedSlnSolution(string path, string content)
        {
            const string patternProjects = "^Project(?!.*(?:" + IGNORED + ")|.*(?:" + FOLDER_NAME + ")).*, \"({.*})\"";

            var projects = Regex.Matches(content, patternProjects, RegexOptions.Multiline);

            // Generate nesting
            var globalSection = new StringBuilder("\tGlobalSection(NestedProjects) = preSolution\r\n");
            foreach (Match g in projects)
            {
                globalSection.AppendLine($"\t\t{g.Groups[1].Captures[0].Value} = {FOLDER_ID}");
            }

            globalSection.AppendLine("\tEndGlobalSection");

            // Insert folder project
            if (!content.Contains($"\"{FOLDER_NAME}\""))
            {
                content = Regex.Replace(content, "^Global", $"{FOLDER}$&", RegexOptions.Multiline);
            }

            // Insert nesting
            const string patternSectionReplace = @"\tGlobalSection\(NestedProjects\)[\s\S]*EndGlobalSection[\s\S]{1,2}(?=EndGlobal)";
            const string patternSectionAdd = @"(?<=EndGlobalSection)[\s\S]{1,2}(?=EndGlobal)";
            if (content.Contains("GlobalSection(NestedProjects)"))
                content = Regex.Replace(content, patternSectionReplace, $"{globalSection}");
            else
                content = Regex.Replace(content, patternSectionAdd, $"\n{globalSection}$&");
            return content;
        }

    }
}