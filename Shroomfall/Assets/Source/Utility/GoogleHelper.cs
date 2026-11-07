using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Utility
{
    [Serializable]
    public class GoogleTokenResponse
    {
        public string id_token;
        public string access_token;
        public string error;
        public string error_description;
    }

    public static class GoogleHelper
    {
        #region Attributes
        private const int LocalPort = 8080;
        private static readonly string RedirectUri = $"http://localhost:{LocalPort}/";
        private static readonly HttpClient Http = new HttpClient();
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static async Task<string> GetGoogleIdTokenAsync()
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri);
                listener.Start();

                string verifier = CreateRandomBase64Url(32);
                string challenge = CreateCodeChallenge(verifier);
                string state = CreateRandomBase64Url(32);

                string authUrl =
                    "https://accounts.google.com/o/oauth2/v2/auth" +
                    "?client_id=" + Uri.EscapeDataString(Configuration.GOOGLE_CLIENT_ID) +
                    "&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
                    "&response_type=code" +
                    "&scope=" + Uri.EscapeDataString(
                        "openid email profile") +
                    "&code_challenge=" + Uri.EscapeDataString(challenge) +
                    "&code_challenge_method=S256" +
                    "&state=" + Uri.EscapeDataString(state);

                Application.OpenURL(authUrl);

                // Wait for Google's redirect to the local listener.
                var context = await listener.GetContextAsync();
                string returnedState = context.Request.QueryString["state"];
                string code = context.Request.QueryString["code"];
                string error = context.Request.QueryString["error"];
                string html;

                if (!string.IsNullOrEmpty(error))
                    html = "<html><body>Google sign-in failed. " + "Return to the game.</body></html>";
                else if (string.IsNullOrEmpty(code) || !string.Equals(state, returnedState, StringComparison.Ordinal))
                    html = "<html><body>Invalid sign-in response. " + "Return to the game.</body></html>";
                else
                    html = "<html><body>Sign-in response received. " + "You can return to the game.</body></html>";

                byte[] bytes = Encoding.UTF8.GetBytes(html);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                context.Response.Close();
                listener.Stop();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception("Google OAuth failed: " + error);

                if (string.IsNullOrEmpty(code) || !string.Equals(state, returnedState, StringComparison.Ordinal))
                    throw new Exception("Google OAuth callback was invalid.");

                // Exchange the authorization code for the ID token.
                var form = new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = Configuration.GOOGLE_CLIENT_ID,
                    ["client_secret"] = Configuration.GOOGLE_CLIENT_SECRET,
                    ["redirect_uri"] = RedirectUri,
                    ["grant_type"] = "authorization_code",
                    ["code_verifier"] = verifier
                };

                using (var content = new FormUrlEncodedContent(form))
                using (var response = await Http.PostAsync("https://oauth2.googleapis.com/token", content))
                {
                    string json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Google token exchange failed.");

                    var tokens = JsonUtility.FromJson<GoogleTokenResponse>(json);
                    if (tokens == null || string.IsNullOrEmpty(tokens.id_token))
                        throw new Exception("Google response did not contain an ID token.");

                    int dots = 0;
                    foreach (char c in tokens.id_token)
                        if (c == '.') dots++;

                    return tokens.id_token;
                }
            }
        }

        private static string CreateRandomBase64Url(
            int byteCount)
        {
            byte[] bytes = new byte[byteCount];

            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            return Base64UrlEncode(bytes);
        }

        private static string CreateCodeChallenge(
            string verifier)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(
                    Encoding.ASCII.GetBytes(verifier));

                return Base64UrlEncode(hash);
            }
        }

        private static string Base64UrlEncode(
            byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        #endregion
    }
}