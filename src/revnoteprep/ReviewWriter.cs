using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Octokit;

namespace ReviewNotesPreparationTool
{
    internal static class ReviewWriter
    {
        public static async Task WriteReviewAsync(ReviewSettings settings)
        {
            var headerValue = new ProductHeaderValue("revnotegen");
            var github = new GitHubClient(headerValue);

            var request = new RepositoryIssueRequest();
            request.State = ItemState.Open;
            request.SortProperty = IssueSort.Created;
            request.SortDirection = SortDirection.Ascending;
            request.Labels.Add(settings.Label);

            var issues = await github.Issue.GetForRepository(settings.Organization, settings.Repository, request);

            var pullRequests = issues.Where(i => i.PullRequest != null);
            var proposals = issues.Where(i => i.PullRequest == null);
            var allIssues = pullRequests.Concat(proposals).ToArray();

            using (var writer = new StreamWriter(settings.OutputFileName))
                WriteReview(writer, allIssues);
        }

        public static void WriteReview(TextWriter writer, IReadOnlyCollection<Issue> issues)
        {
            writer.WriteLine("# API Review {0}-{1:00}-{2:00}", DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            writer.WriteLine();
            writer.WriteLine("This API review was also recorded and is available on [Google Hangouts](hangouturl)");
            writer.WriteLine();

            writer.WriteLine("## Overview");
            writer.WriteLine();
            writer.WriteLine("In this API review we reviewed the following PRs and proposals:");
            writer.WriteLine();

            foreach (var issue in issues)
                writer.WriteLine("* [Notes](#{0}) | [#{1}]({2}) {3}", GetAnchor(issue),  issue.Number, issue.HtmlUrl, GetEscapedTitle(issue));

            writer.WriteLine();
            writer.WriteLine("There are also some [follow-ups for the API review board](#follow-ups-for-api-review-board).");
            writer.WriteLine();
            writer.WriteLine("## Notes");

            foreach (var issue in issues)
            {
                writer.WriteLine();
                writer.WriteLine("### #{0} {1}", issue.Number, GetEscapedTitle(issue));
                writer.WriteLine();
                writer.WriteLine("Status: **Not reviewed** |");
                writer.WriteLine("[Issue]({0}) |", issue.HtmlUrl);
                if (issue.PullRequest != null)
                    writer.WriteLine("[API Reference](issue-#{0}.md) |", issue.Number);
                writer.WriteLine("[Video](hangouturl)");
                writer.WriteLine();
                writer.WriteLine("* Notes");
            }

            writer.WriteLine();
            writer.WriteLine("### Follow-ups for API review board");
            writer.WriteLine();
            writer.WriteLine("* Notes");
        }

        private static string GetEscapedTitle(Issue issue)
        {
            return issue.Title.Replace("<", @"\<");
        }

        private static string GetTitle(Issue issue)
        {
            return String.Format("#{0} {1}", issue.Number, issue.Title);
        }

        private static string GetAnchor(Issue issue)
        {
            var title = GetTitle(issue);
            var sb = new StringBuilder(title.Length);

            var lastIsDash = true;

            for (int i = 0; i < title.Length; i++)
            {
                var c = title[i];
                if (Char.IsLetterOrDigit(c))
                {
                    sb.Append(Char.ToLower(c));
                    lastIsDash = false;
                }
                else if (Char.IsWhiteSpace(c) && !lastIsDash && i < title.Length - 1)
                {
                    sb.Append('-');
                    lastIsDash = true;
                }
            }

            return sb.ToString();
        }
    }
}