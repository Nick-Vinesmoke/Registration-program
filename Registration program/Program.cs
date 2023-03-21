using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace RegistrationProgram
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Registration Program";
        static string FileName = "registration_data.txt"; // Name of the file to store registration data

        static DriveService GetDriveService()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        static void Login()
        {
            Console.WriteLine("Welcome to the registration program!");
            Console.Write("Please enter your name: ");
            string name = Console.ReadLine();

            Console.Write("Please enter your age: ");
            int age = int.Parse(Console.ReadLine());

            Console.Write("Please enter your email address: ");
            string email = Console.ReadLine();

            Console.Write("Please enter your password: ");
            string password = Console.ReadLine();

            // Save registration data to a text file in Google Drive
            DriveService service = GetDriveService();
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = FileName,
                MimeType = "text/plain"
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(name);
                writer.WriteLine(age);
                writer.WriteLine(email);
                writer.WriteLine(password);
                writer.Flush();
                stream.Position = 0;
                request = service.Files.Create(fileMetadata, stream, "text/plain");
                request.Fields = "id";
                request.Upload();
            }
            Console.WriteLine("Registration completed successfully!");
        }

        static void Signin()
        {
            Console.WriteLine("Welcome to the login program!");
            Console.Write("Please enter your email address: ");
            string email = Console.ReadLine();

            Console.Write("Please enter your password: ");
            string password = Console.ReadLine();

            // Check if the registration data exists in the text file in Google Drive
            DriveService service = GetDriveService();
            var fileList = service.Files.List().Execute().Files;
            var file = fileList.Find(f => f.Name == FileName);
            if (file != null)
            {
                FilesResource.DownloadRequest request = service.Files.Download(file.Id);
                MemoryStream stream = new MemoryStream();
                request.Download(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == email)
                        {
                            if (reader.ReadLine() == password)
                            {
                                Console.WriteLine("Login successful!");
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Invalid password.");
                                return;
                            }
                        }
                    }
                    Console.WriteLine("Email not found.");
                }
            }
            else
            {
                Console.WriteLine("Registration data file not found.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("!Please select an option!");
            Console.WriteLine("1.log in");
            Console.WriteLine("2.sign in");
            string choose = Console.ReadLine();
            if (choose == "1")
            {
                Login();
            }
            else if (choose == "2")
            {
                Signin();
            }

        }
    }
}
