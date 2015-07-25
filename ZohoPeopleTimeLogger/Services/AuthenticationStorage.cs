using System;
using System.IO;
using System.Xml.Serialization;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public class AuthenticationStorage : IAuthenticationStorage
    {
        private readonly string authenticationStorageFolder;
        private readonly string authenticationStorageFilePath;

        public AuthenticationStorage()
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

        public void SaveAuthenticationData(AuthenticationData data)
        {
            if (!Directory.Exists(authenticationStorageFolder))
            {
                Directory.CreateDirectory(authenticationStorageFolder);
            }

            using (var file = File.Open(authenticationStorageFilePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(AuthenticationData));
                serializer.Serialize(file, data);
            }
        }

        public void Clear()
        {
            if (File.Exists(authenticationStorageFilePath))
            {
                File.Delete(authenticationStorageFilePath);
            }
        }
    }
}