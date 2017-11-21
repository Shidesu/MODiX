﻿using Discord;
using Modix.Data.Utilities;
using Modix.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.AutoCodePaste
{
    public class CodePasteService
    {
        private const string Header = @"{0}

/*
    Written By: {1} in #{2}
    Posted on {3}
    Message ID: {4}
*/";

        private const string _ApiReferenceUrl = "https://hastebin.com/";

        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        private static async Task<string> UploadCode(string code)
        {
            var client = new HttpClient();
            var response = await client.PostAsync($"{_ApiReferenceUrl}documents", FormatUtilities.BuildContent(code));

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while posting code to Hastebin.");
            }

            string urlResponse = await response.Content.ReadAsStringAsync();
            string pasteKey = JObject.Parse(urlResponse)["key"].Value<string>();

            return $"{_ApiReferenceUrl}{pasteKey}.cs";
        }

        /// <summary>
        /// Uploads the code in the given message to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="msg">The Discord message to upload</param>
        /// <param name="code">The string to upload instead of message content</param>
        /// <returns>The URL to the newly created post</returns>
        internal static async Task<string> UploadCode(IMessage msg, string code = null)
        {
            string formatted = String.Format(FormatUtilities.FixIndentation(code ?? msg.Content), Header, 
                $"{msg.Author.Username}#{msg.Author.DiscriminatorValue}", msg.Channel.Name,
                DateTime.Now.ToString("dddd, MMMM d yyyy @ H:mm:ss"), msg.Id);

            return await UploadCode(formatted);
        }

        internal static EmbedBuilder BuildEmbed(IUser user, string content, string url)
        {
            string cleanCode = FormatUtilities.FixIndentation(content);

            return new EmbedBuilder()
                .WithTitle("Your message was re-uploaded")
                .WithAuthor(user)
                .WithDescription(cleanCode.Trim().Truncate(300))
                .AddInlineField("Auto-Paste", url)
                .WithColor(new Color(95, 186, 125));
        }
    }
}