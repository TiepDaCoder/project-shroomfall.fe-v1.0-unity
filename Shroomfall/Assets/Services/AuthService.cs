using Assets.Utilities;
using Contract.DTO.Feature.Identity.Command;
using Contract.DTO.Feature.Identity.Response;
using System;
using System.Threading.Tasks;
using UnityEngine.Analytics;

namespace Assets.Services
{
    public class AuthService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public TokenDTO Token { get; private set; }
        public bool IsInitialized { get; private set; } = false;
        #endregion

        public AuthService()
        {

        }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        #region Senders
        public async Task Login(
            string email,
            string password)
        {
            var dto = new LoginDTO
            {
                Email = email,
                Password = password
            };

            var result = await HttpCaller.PostAsync<LoginDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}login",
                dto
            );

            ValidateToken(result);

            ConfigToken(result);
        }

        public async Task Register(
            string email,
            string password,
            string fullName)
        {
            var dto = new RegisterDTO
            {
                Email = email,
                Password = password,
                Name = fullName,
            };

            var result = await HttpCaller.PostAsync<RegisterDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}register",
                dto
            );

            ValidateToken(result);

            ConfigToken(result);
        }

        public async Task<bool> Refresh()
        {
            try
            {
                // Prime the HTTP client header with the old access token so the backend permits the call
                HttpCaller.SetBearerToken(Token.AccessToken);

                var dto = new RefreshTokenDTO
                {
                    RefreshToken = Token.RefreshToken
                };

                var result = await HttpCaller.PostAsync<RefreshTokenDTO, TokenDTO>(
                    $"{Configuration.IDENTITY_CONTROLLER}refresh",
                    dto
                );

                ValidateToken(result);

                ConfigToken(result);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateProfile(
            string name,
            DateTime dob)
        {
            var dto = new UpdateProfileDTO
            {
                Dob = dob,
                Gender = Gender.Unknown.ToString(),
                Name = name
            };

            await HttpCaller.PutAsync<object, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}profile",
                dto);
        }

        public async Task SteamAuth()
        {
            var steamTicket = SteamHelper.GetAuthTicket();

            var dto = new SteamAuthDTO
            {
                SteamTicket = steamTicket,
                SteamName = SteamHelper.SteamName,
            };

            var result = await HttpCaller.PostAsync<SteamAuthDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}steam",
                dto
            );

            SteamHelper.CancelTicket();

            ValidateToken(result);

            ConfigToken(result);
        }

        private static void ValidateToken(
            TokenDTO token)
        {
            if (token == null)
                throw new Exception("Token response is null");

            if (string.IsNullOrWhiteSpace(token.AccessToken))
                throw new Exception("Access token is missing");
        }
        #endregion

        #region Receivers
        #endregion

        private void ConfigToken(
            TokenDTO token)
        {
            // Configure http
            HttpCaller.SetBearerToken(token.AccessToken);

            // Apply runtime
            if (Token == token)
                return;

            Token = token;
        }
        #endregion
    }
}