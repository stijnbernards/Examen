using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rest.Web;
// ReSharper disable All

namespace Rest.Server.Files
{
    public class HTMLFile : FileBase
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();

        public void AddData(Dictionary<string, object> data)
        {
            foreach (KeyValuePair<string, object> item in data)
            {
                variables.Add(item.Key, item.Value);
            }
        }

        public string GetData()
        {
            MatchIf();
            MatchForeach();
            MatchVariables();
            return data;
        }

        private void MatchForeach()
        {
            Regex foreachRegex = new Regex(@"{{*\s*foreach*\s*\((.*?) in (.*?)\)*\s*}}(.*?){{*\s*endforeach*\s*}}", RegexOptions.Singleline);
            Regex variableRegex = new Regex(@"{{(.*?)}}", RegexOptions.IgnoreCase);

            MatchCollection matches = foreachRegex.Matches(data);

            if (matches.Count <= 0) return;

            foreach (Match match in matches)
            {
                if (match.Groups.Count != 4) continue;

                string replace = match.Groups[1].Value.Trim();
                string completedForeach = "";


                if(!variables.ContainsKey(match.Groups[2].Value.Trim()) || !(variables[match.Groups[2].Value.Trim()].GetType() == typeof(string[]) || variables[match.Groups[2].Value.Trim()].GetType() == typeof(List<string>)))
                {
                    data = data.Replace(match.Value,
                        "Variables supplied in foreach: " + match.Value + " do not exist or are invalid");

                    continue;
                }

                MatchCollection variablesMatches = variableRegex.Matches(match.Groups[3].Value);

                if (variablesMatches.Count > 0)
                {
                    foreach (string variable in variables[match.Groups[2].Value.Trim()] as string[])
                    {
                        string result = match.Groups[3].Value;

                        foreach (Match variableMatch in variablesMatches)
                        {
                            string newMatch = RemoveOperators(variableMatch.Value);

                            if (newMatch == replace)
                            {
                                result = result.Replace(variableMatch.Value, variable);
                            }
                        }

                        completedForeach += result;
                    }
                }

                data = data.Replace(match.Value, completedForeach);
            }
        }

        private void MatchVariables()
        {
            Regex variableRegex = new Regex(@"{{(.*?)}}", RegexOptions.IgnoreCase);
            MatchCollection matches = variableRegex.Matches(data);

            string newMatch = "";

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    newMatch = RemoveOperators(match.Value);

                    if (variables.ContainsKey(newMatch))
                    {
                        data = data.Replace(match.Value, variables[newMatch].ToString());
                    }
                    else
                    {
                        data = data.Replace(match.Value, "Variable: " + newMatch + " Not found!");
                    }
                }
            }
        }

        private void MatchIf()
        {
            Regex ifRegex = new Regex(@"{{*\s*if*\s*\((.*?)\)*\s*}}(.*?){{*\s*endif*\s*}}", RegexOptions.Singleline);
            MatchCollection matches = ifRegex.Matches(data);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string variable = match.Groups[1].Value.Replace("!", "");

                    if (match.Groups[1].Value.StartsWith("!"))
                    {
                        if (!variables.ContainsKey(variable))
                        {
                            data = data.Replace(match.Value, match.Groups[2].Value);
                        }
                        else
                        {
                            data = data.Replace(match.Value, "");
                        }
                    }
                    else
                    {
                        if (variables.ContainsKey(variable))
                        {
                            data = data.Replace(match.Value, match.Groups[2].Value);
                        }
                        else
                        {
                            data = data.Replace(match.Value, "");
                        }
                    }
                }
            }
        }

        private string RemoveOperators(string value)
        {
            return value.Replace("{{", "").Replace("}}", "").Trim();
        }

        public HTMLFile(string filePath) : base(filePath)
        {
        }

        public HTMLFile(FileBase fb) : base(fb)
        {
        }
    }
}