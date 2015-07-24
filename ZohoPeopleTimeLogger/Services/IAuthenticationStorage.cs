using System.Security.Cryptography.X509Certificates;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IAuthenticationStorage
    {
        AuthenticationData GetAuthenticationData();

        void SaveAuthenticationData(AuthenticationData data);
    }
}