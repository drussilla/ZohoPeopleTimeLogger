using System;
using System.IO;
using System.Xml.Serialization;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string authenticationStorageFolder;
        private readonly string authenticationStorageFilePath;

        public AuthenticationService()
        {
            authenticationStorageFolder = 
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ZohoPeopleTimeLogger");
            authenticationStorageFilePath =
                Path.Combine(
                    authenticationStorageFolder,
                    "autheentication.xml");
        }

        public AuthenticationData GetAuthenticationData()
        {
            if (!File.Exists(authenticationStorageFilePath))
            {
                return null;
            }

            using (var file = File.OpenRead(authenticationStorageFilePath))
            {
                var serializer = new XmlSerializer(typeof(AuthenticationData));
                return serializer.Deserialize(file) as AuthenticationData;
            }
        }

        public void SaveAuthenticationData(string userName, string token)
        {
            if (!Directory.Exists(authenticationStorageFolder))
            {
                Directory.CreateDirectory(authenticationStorageFolder);
            }

            using (var file = File.Open(authenticationStorageFilePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(AuthenticationData));
                serializer.Serialize(file, new AuthenticationData {UserName = userName, Token = token});
            }
        }
    }
}