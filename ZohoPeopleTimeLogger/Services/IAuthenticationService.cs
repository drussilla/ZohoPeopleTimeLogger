using System.Security.Cryptography.X509Certificates;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IAuthenticationService
    {
        AuthenticationData GetAuthenticationData();

        void SaveAuthenticationData(string userName, string token);
    }
}