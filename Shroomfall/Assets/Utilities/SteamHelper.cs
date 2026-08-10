using Steamworks;
using System;

namespace Assets.Utilities
{
    public static class SteamHelper
    {
        #region Attributes
        private static HAuthTicket currentTicket;
        #endregion

        #region Properties
        public static ulong SteamId => SteamUser.GetSteamID().m_SteamID;
        public static string SteamName => SteamFriends.GetPersonaName();
        #endregion

        #region Methods
        public static string GetAuthTicket()
        {
            if (currentTicket != HAuthTicket.Invalid)
            {
                SteamUser.CancelAuthTicket(currentTicket);
                currentTicket = HAuthTicket.Invalid;
            }

            byte[] buffer = new byte[1024];

            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();

            currentTicket = SteamUser.GetAuthSessionTicket(
                buffer,
                buffer.Length,
                out uint ticketSize,
                ref identity);

            return BitConverter.ToString(buffer, 0, (int)ticketSize).Replace("-", "");
        }

        public static void CancelTicket()
        {
            SteamUser.CancelAuthTicket(currentTicket);
        }
        #endregion
    }
}