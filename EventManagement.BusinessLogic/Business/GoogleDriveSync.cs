using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

namespace EventManagement.BusinessLogic.Business
{
    public class GoogleDriveSync
    {
        public static GoogleCredential _cred;
        
        private static DriveService service;
        
        public static string jsonkeysfile = WebConfigurationManager.AppSettings["jsonkeysfile"];
        
        // private static string jsonkeysfilepath = Application.StartupPath + @"\" + jsonkeysfile;
        //private static string filetoupload_path = Application.StartupPath + @"\" + "Vista.jpg";
        
        public static string owneremail = WebConfigurationManager.AppSettings["owneremail"];

        /// <summary>  
        /// Get Access Token From JSON Key Async  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string Task</returns>  
        
        public static async Task<string> GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath, params string[] scopes)
        {
            try
            {
                // Get active credential
                using (var stream = new System.IO.FileStream(jsonKeyFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    _cred = GoogleCredential.FromStream(stream);
                    _cred = _cred.CreateScoped(scopes);
                    service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = _cred,
                        ApplicationName = "Drive API Sample",
                    });
                    try
                    {
                        Task<string> task = ((ITokenAccess)_cred).GetAccessTokenForRequestAsync();
                        task.Wait();
                        string bearer = task.Result;
                        return bearer;
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>  
        /// Get Access Token From JSON Key  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string</returns>  
        
        public static string GetAccessTokenFromJSONKey(string jsonKeyFilePath, params string[] scopes)
        {
            return GetAccessTokenFromJSONKeyAsync(jsonKeyFilePath, scopes).Result;
        }

        /// 
        /// List all of the files and directories for the current user.  
        /// 
        /// Documentation: https://developers.google.com/drive/v2/reference/files/list
        /// Documentation Search: https://developers.google.com/drive/web/search-parameters
        /// 
        ///a Valid authenticated DriveService        
        ///if Search is null will return all files        
        /// 
        public static List<File> GetFiles(DriveService service, string search = "")
        {

            List<File> Files = new List<File>();

            try
            {
                //List all of the files and directories for the current user.  
                // Documentation: https://developers.google.com/drive/v2/reference/files/list
                FilesResource.ListRequest list = service.Files.List();
                list.PageSize = 1000;
                if (search != null && search.Trim().Length > 0)
                {
                    list.Q = search;
                }
                FileList filesFeed = list.Execute();

                //// Loop through until we arrive at an empty page
                while (filesFeed.Files != null)
                {
                    // Adding each item  to the list.
                    foreach (File item in filesFeed.Files)
                    {
                        Files.Add(item);
                    }

                    // We will know we are on the last page when the next page token is
                    // null.
                    // If this is the case, break.
                    if (filesFeed.NextPageToken == null)
                    {
                        break;
                    }

                    // Prepare the next page of results
                    list.PageToken = filesFeed.NextPageToken;

                    // Execute and process the next page request
                    filesFeed = list.Execute();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Files;
        }

        /// 
        /// List all of the files and directories for the current user.  
        /// 
        /// Documentation: https://developers.google.com/drive/v2/reference/files/list
        /// Documentation Search: https://developers.google.com/drive/web/search-parameters
        /// 
        ///a Valid authenticated DriveService        
        ///if Search is null will return all files        
        /// 
        public static List<File> CheckFolderExists(DriveService service, string FolderName)
        {

            List<File> Files = new List<File>();

            try
            {
                //List all of the files and directories for the current user.  
                // Documentation: https://developers.google.com/drive/v2/reference/files/list
                FilesResource.ListRequest list = service.Files.List();
                list.PageSize = 1000;
                if (FolderName != null && FolderName.Trim().Length > 0)
                {
                    list.Q = "mimeType = 'application/vnd.google-apps.folder' and name = '" + FolderName + "'";
                }
                FileList filesFeed = list.Execute();

                //// Loop through until we arrive at an empty page
                while (filesFeed.Files != null)
                {
                    // Adding each item  to the list.
                    foreach (File item in filesFeed.Files)
                    {
                        Files.Add(item);
                    }

                    // We will know we are on the last page when the next page token is
                    // null.
                    // If this is the case, break.
                    if (filesFeed.NextPageToken == null)
                    {
                        break;
                    }

                    // Prepare the next page of results
                    list.PageToken = filesFeed.NextPageToken;

                    // Execute and process the next page request
                    filesFeed = list.Execute();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Files;
        }

        // DriveService _service: Valid, authenticated Drive service
        // string_ title: Title of the folder
        // string _description: Description of the folder
        // _parent: ID of the parent directory to which the folder should be created

        public static File CreateDirectory(DriveService _service, string _foldername, string _folderdescription, string _parent)
        {
            File NewDirectory = null;
            try
            {
                File body = new File
                {
                    Name = _foldername,
                    Description = _folderdescription,
                    MimeType = "application/vnd.google-apps.folder",
                    Permissions = new List<Permission>(),
                    //body.Permissions = new List<Permission> { new Permission(). };
                    //Parent folder or pass 'root' to create on root
                    Parents = new List<string>() { _parent }
                };
                FilesResource.CreateRequest request = _service.Files.Create(body);
                NewDirectory = request.Execute();
                InsertPermission(_service, NewDirectory);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewDirectory;
        }

        /// 

        // tries to figure out the mime type of the file.
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        /// Uploads a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// 
        /// a Valid authenticated DriveService
        /// path to the file to upload
        /// Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.
        /// If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null
        public static File UploadFile(DriveService _service, string _uploadFile, string _parent)
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File
                {
                    Name = System.IO.Path.GetFileName(_uploadFile),
                    Description = "File uploaded by Drive Sample",
                    MimeType = GetMimeType(_uploadFile),
                    Parents = new List<string>() { _parent }
                };
                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    InsertPermission(_service, request.ResponseBody);
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }

        /// 

        /// Updates a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/update
        /// 
        /// a Valid authenticated DriveService
        /// path to the file to upload
        /// Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.
        /// the resource id for the file we would like to update                      
        /// If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null
        public static File UpdateFile(DriveService _service, string _uploadFile, string _parent, string _fileId)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File
                {
                    Name = System.IO.Path.GetFileName(_uploadFile),
                    Description = "File updated by Diamto Drive Sample",
                    MimeType = GetMimeType(_uploadFile),
                    Parents = new List<string> { _parent }
                };
                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = _service.Files.Update(body, _fileId, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    InsertPermission(_service, request.ResponseBody);
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }
        public static void CallSaveApi(string jsonkeysfilepath, string filename, string imagepath)
        {
            try
            {
                var token = GetAccessTokenFromJSONKey(jsonkeysfilepath, new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile });
                Console.WriteLine("Token: " + token);

                string foldername = filename;
                File testfolder = null;
                //Check Folder Exists
                List<File> _folderexists = CheckFolderExists(service, foldername);
                if (_folderexists == null || _folderexists.Count == 0)
                    //Create Folder
                    testfolder = CreateDirectory(service, foldername, "testing drive api", "root");
                else
                    testfolder = _folderexists[0];

                string parentid = testfolder.Id;

                //Upload File
                File resp = UploadFile(service, imagepath, parentid);

                //Get All Files/Selected(filter) files from google-drive
                //List<File> files = GetFiles(service, "");

                //Delete File/Folder
                //DeleteFilePermanently(service, IndFile);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void CallApi(string jsonkeysfilepath, string filename)
        {
            try
            {
                var token = GetAccessTokenFromJSONKey(jsonkeysfilepath, new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile });
                Console.WriteLine("Token: " + token);

                string foldername = filename;
                File testfolder = null;
                //Check Folder Exists
                List<File> _folderexists = CheckFolderExists(service, foldername);
                if (_folderexists == null || _folderexists.Count == 0)
                    //Create Folder
                    testfolder = CreateDirectory(service, foldername, "testing drive api", "root");
                else
                    testfolder = _folderexists[0];

                string parentid = testfolder.Id;

                //Upload File
                //File resp = UploadFile(service, filetoupload_path, parentid);

                //Get All Files/Selected(filter) files from google-drive
                List<File> files = GetFiles(service, string.Empty);

                //Delete File/Folder
                //DeleteFilePermanently(service, IndFile);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ///Deletes the file permanently 
        private static string DeleteFilePermanently(DriveService service, File _file)
        {
            string Resp = null;
            try
            {
                FilesResource.DeleteRequest DeleteRequest = service.Files.Delete(_file.Id);
                Resp = DeleteRequest.Execute();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Resp;
        }

        /// <summary>
        /// Insert a new permission.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="fileId">ID of the file to insert permission for.</param>
        /// <param name="who">
        /// User or group e-mail address, domain name or null for "default" type.
        /// </param>
        /// <param name="type">The value "user", "group", "domain" or "default".</param>
        /// <param name="role">The value "owner", "writer" or "reader".</param>
        /// <returns>The inserted permission, null is returned if an API error occurred</returns>
        public static Permission InsertPermission(DriveService service, File file)
        {
            Permission newPermission = new Permission
            {
                Role = "writer",
                EmailAddress = owneremail,
                Type = "user"
            };
            try
            {
                PermissionsResource.CreateRequest cc = new PermissionsResource.CreateRequest(service, newPermission, file.Id);
                //cc.TransferOwnership = true; // acknowledge transfer of ownership - must be set to "true" in order for role to change to "owner"
                return cc.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }
    }
}
