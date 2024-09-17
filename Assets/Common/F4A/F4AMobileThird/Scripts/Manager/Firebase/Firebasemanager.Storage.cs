namespace com.F4A.MobileThird
{
#if DEFINE_FIREBASE_STORAGE
    using Firebase.Extensions;
    using Firebase.Storage;
#endif
    using Firebase;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using UnityEngine;
    using System.Text.RegularExpressions;
    using System.Globalization;

    public partial class FirebaseManager
    {
        protected string MyStorageBucket = "gs://art-story-701be.appspot.com/";
        private const int kMaxLogSize = 16382;
        protected static string UriFileScheme = Uri.UriSchemeFile + "://";

        // Cloud Storage location to download from / upload to.
        protected string storageLocation;
        // String to upload to storageLocation or the contents of the file downloaded from
        // storageLocation.
        protected string fileContents;
        // Used to keep track of changes to fileContents for display in the UI.
        protected string previousFileContents;
        // Section of the file that can be edited by the user.
        protected string editableFileContents;
        // Metadata to change when uploading a file.
        protected string fileMetadataChangeString = "";
        // Local file to upload from / download to.
        public string bookFilename = "BookData.json";
        public string partFilename = "ChapterData.json";
        public string puzzleFilename = "PuzzleData.json";
        public string puzzleCollectionFilename = "PuzzlesCollection.json";
        public string LiveEventPostCardname = "LiveEventPostCard.json";
        public string PuzzlesInPostcardname = "PuzzlesInPostcard.json";
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        protected bool isFirebaseInitialized = false;
        // Hold a reference to the FirebaseStorage object so that we're not reinitializing the API on
        // each transfer.
        protected FirebaseStorage storage;
        // Currently enabled logging verbosity.
        protected Firebase.LogLevel logLevel = Firebase.LogLevel.Info;
        // Whether an operation is in progress.
        protected bool operationInProgress;
        // Cancellation token source for the current operation.
        protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        // Cached path to the persistent data directory as Application.persistentDataPath can only be used from the main
        // Unity thread.
        protected string persistentDataPath;

        // Previously completed task.
        protected Task previousTask;

        // When the app starts, check to make sure that we have
        // the required dependencies to use Firebase, and if not,
        // add them if possible.

        protected virtual void InitializeStorage()
        {
            var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
            storage = FirebaseStorage.DefaultInstance;
            if (!String.IsNullOrEmpty(appBucket))
            {
                MyStorageBucket = String.Format("gs://{0}/", appBucket);
            }
            storage.LogLevel = logLevel;
            isFirebaseInitialized = true;
        }

        // Retrieve a storage reference from the user specified path.
        protected StorageReference GetStorageReference()
        {

            // If this is an absolute path including a bucket create a storage instance.
            if (storageLocation.StartsWith("gs://") ||
                storageLocation.StartsWith("http://") ||
                storageLocation.StartsWith("https://"))
            {
                var storageUri = new Uri(storageLocation);
                var firebaseStorage = FirebaseStorage.GetInstance(
                  String.Format("{0}://{1}", storageUri.Scheme, storageUri.Host));
                return firebaseStorage.GetReferenceFromUrl(storageLocation);
            }
            // When using relative paths use the default storage instance which uses the bucket supplied
            // on creation of FirebaseApp.
            return FirebaseStorage.DefaultInstance.GetReference(storageLocation);
        }

        // Get the local filename as a URI relative to the persistent data path if the path isn't
        // already a file URI.
        protected virtual string PathToPersistentDataPathUriString(string filename)
        {
            if (filename.StartsWith(UriFileScheme))
            {
                return filename;
            }
            return String.Format("{0}{1}/{2}", UriFileScheme, persistentDataPath,
                                 filename);
        }

        // Cancel the currently running operation.
        protected void CancelOperation()
        {
            if (operationInProgress && cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }

        // Display a storage exception.
        protected void DisplayStorageException(Exception exception)
        {
            var storageException = exception as StorageException;
            if (storageException != null)
            {
            }
            else
            {
            }
        }

        // Display the result of an upload operation.
        protected void DisplayUploadComplete(Task<StorageMetadata> task)
        {
            if (!(task.IsFaulted || task.IsCanceled))
            {
                fileContents = "";
                fileMetadataChangeString = "";
            }
        }

        // Write upload state to the log.
        protected virtual void DisplayUploadState(UploadState uploadState)
        {
            if (operationInProgress)
            {
                Debug.Log(String.Format("Uploading {0}: {1} out of {2}", uploadState.Reference.Name,
                                       uploadState.BytesTransferred, uploadState.TotalByteCount));
            }
        }

        // Upload file text to Cloud Storage using a byte array.
        public IEnumerator UploadBytes()
        {
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Uploading to {0} ...", storageReference.Path));
            var task = storageReference.PutBytesAsync(
              Encoding.UTF8.GetBytes(fileContents), StringToMetadataChange(fileMetadataChangeString),
              new StorageProgress<UploadState>(DisplayUploadState),
              cancellationTokenSource.Token, null);
            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }

        // Upload file to Cloud Storage using a stream.
        public IEnumerator UploadStream()
        {
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Uploading to {0} using stream...", storageReference.Path));
            var task = storageReference.PutStreamAsync(
              new MemoryStream(System.Text.Encoding.ASCII.GetBytes(fileContents)),
              StringToMetadataChange(fileMetadataChangeString),
              new StorageProgress<UploadState>(DisplayUploadState),
              cancellationTokenSource.Token, null);
            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }

        // Upload a file from the local filesystem to Cloud Storage.
        public IEnumerator UploadFromFile()
        {
            var localFilenameUriString = PathToPersistentDataPathUriString(bookFilename);
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Uploading '{0}' to '{1}'...", localFilenameUriString,
                                   storageReference.Path));
            var task = storageReference.PutFileAsync(
              localFilenameUriString, StringToMetadataChange(fileMetadataChangeString),
              new StorageProgress<UploadState>(DisplayUploadState),
              cancellationTokenSource.Token, null);
            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }

        // Update the metadata on the file in Cloud Storage.
        public IEnumerator UpdateMetadata()
        {
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Updating metadata of {0} ...", storageReference.Path));
            var task = storageReference.UpdateMetadataAsync(StringToMetadataChange(
              fileMetadataChangeString));
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                Debug.Log("Updated metadata");
                Debug.Log(MetadataToString(task.Result, false) + "\n");
            }
        }

        // Write download state to the log.
        public virtual void DisplayDownloadState(DownloadState downloadState)
        {
            if (operationInProgress)
            {
                Debug.Log(String.Format("Downloading {0}: {1} out of {2}", downloadState.Reference.Name,
                                       downloadState.BytesTransferred, downloadState.TotalByteCount));
            }
        }

        // Download from Cloud Storage into a byte array.
        public IEnumerator DownloadBytes(string fileName, Action<string> onComplete)
        {
            storageLocation = MyStorageBucket + fileName;
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Downloading {0} ...", storageReference.Path));
            var task = storageReference.GetBytesAsync(
              0, new StorageProgress<DownloadState>(DisplayDownloadState),
              cancellationTokenSource.Token);
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                fileContents = System.Text.Encoding.Default.GetString(task.Result);
                onComplete?.Invoke(fileContents);
            }
        }

        // Download from Cloud Storage using a stream.
        public IEnumerator DownloadStreams(string fileName, Action<string> onComplete)
        {
            storageLocation = MyStorageBucket + fileName;
            // Download the file using a stream.
            fileContents = "";
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Downloading {0} with stream ...", storageReference.Path));
            var task = storageReference.GetStreamAsync((stream) =>
            {
                var buffer = new byte[1024];
                int read;
                // Read data to render in the text view.
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileContents += System.Text.Encoding.Default.GetString(buffer, 0, read);
                }
            },
              new StorageProgress<DownloadState>(DisplayDownloadState),
              cancellationTokenSource.Token);

            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                Debug.Log("Finished downloading stream\n");
                onComplete?.Invoke(fileContents);
            }
        }

        // Get a local filesystem path from a file:// URI.
        public string FileUriStringToPath(string fileUriString)
        {
            return Uri.UnescapeDataString((new Uri(fileUriString)).PathAndQuery);
        }

        // Download from Cloud Storage to a local file.
        public IEnumerator DownloadToFile(string fileName)
        {
            storageLocation = MyStorageBucket + fileName;
            var storageReference = GetStorageReference();
            var localFilenameUriString = PathToPersistentDataPathUriString(bookFilename);
            Debug.Log(String.Format("Downloading {0} to {1}...", storageReference.Path,
                                   localFilenameUriString));
            var task = storageReference.GetFileAsync(
              localFilenameUriString,
              new StorageProgress<DownloadState>(DisplayDownloadState),
              cancellationTokenSource.Token);
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                var filename = FileUriStringToPath(localFilenameUriString);
                fileContents = File.ReadAllText(filename);
            }
        }

        // Delete a remote file.
        protected IEnumerator Delete()
        {
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Deleting {0}...", storageReference.Path));
            var task = storageReference.DeleteAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                Debug.Log(String.Format("{0} deleted", storageReference.Path));
            }
        }

        // Download and display Metadata for the storage reference.
        protected IEnumerator GetMetadata()
        {
            var storageReference = GetStorageReference();
            Debug.Log(String.Format("Bucket: {0}", storageReference.Bucket));
            Debug.Log(String.Format("Path: {0}", storageReference.Path));
            Debug.Log(String.Format("Name: {0}", storageReference.Name));
            Debug.Log(String.Format("Parent Path: {0}", storageReference.Parent != null ?
                                                           storageReference.Parent.Path : "(root)"));
            Debug.Log(String.Format("Root Path: {0}", storageReference.Root.Path));
            Debug.Log(String.Format("App: {0}", storageReference.Storage.App.Name));
            var task = storageReference.GetMetadataAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
                Debug.Log(MetadataToString(task.Result, false) + "\n");
        }

        // Display the download URL for a storage reference.
        public IEnumerator ShowDownloadUrl()
        {
            var task = GetStorageReference().GetDownloadUrlAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                Debug.Log(String.Format("DownloadUrl={0}", task.Result));
            }
        }

        // Wait for task completion, throwing an exception if the task fails.
        // This could be typically implemented using
        // yield return new WaitUntil(() => task.IsCompleted);
        // however, since many procedures in this sample nest coroutines and we want any task exceptions
        // to be thrown from the top level coroutine (e.g UploadBytes) we provide this
        // CustomYieldInstruction implementation wait for a task in the context of the coroutine using
        // common setup and tear down code.
        class WaitForTaskCompletion : CustomYieldInstruction
        {
            Task task;
            FirebaseManager uiHandler;

            // Create an enumerator that waits for the specified task to complete.
            public WaitForTaskCompletion(FirebaseManager uiHandler, Task task)
            {
                uiHandler.previousTask = task;
                uiHandler.operationInProgress = true;
                this.uiHandler = uiHandler;
                this.task = task;
            }

            // Wait for the task to complete.
            public override bool keepWaiting
            {
                get
                {
                    if (task.IsCompleted)
                    {
                        uiHandler.operationInProgress = false;
                        uiHandler.cancellationTokenSource = new CancellationTokenSource();
                        if (task.IsFaulted)
                        {
                            uiHandler.DisplayStorageException(task.Exception);
                        }
                        return false;
                    }
                    return true;
                }
            }
        }

        // Convert a string in the form:
        //   key1=value1
        //   ...
        //   keyN=valueN
        //
        // to a MetadataChange object.
        //
        // If an empty string is provided this method returns null.
        MetadataChange StringToMetadataChange(string metadataString)
        {
            var metadataChange = new MetadataChange();
            var customMetadata = new Dictionary<string, string>();
            bool hasMetadata = false;
            foreach (var metadataStringLine in metadataString.Split(new char[] { '\n' }))
            {
                if (metadataStringLine.Trim() == "")
                    continue;
                var keyValue = metadataStringLine.Split(new char[] { '=' });
                if (keyValue.Length != 2)
                {
                    Debug.Log(String.Format("Ignoring malformed metadata line '{0}' tokens={2}",
                                           metadataStringLine, keyValue.Length));
                    continue;
                }
                hasMetadata = true;
                var key = keyValue[0];
                var value = keyValue[1];
                if (key == "CacheControl")
                {
                    metadataChange.CacheControl = value;
                }
                else if (key == "ContentDisposition")
                {
                    metadataChange.ContentDisposition = value;
                }
                else if (key == "ContentEncoding")
                {
                    metadataChange.ContentEncoding = value;
                }
                else if (key == "ContentLanguage")
                {
                    metadataChange.ContentLanguage = value;
                }
                else if (key == "ContentType")
                {
                    metadataChange.ContentType = value;
                }
                else
                {
                    customMetadata[key] = value;
                }
            }
            if (customMetadata.Count > 0)
                metadataChange.CustomMetadata = customMetadata;
            return hasMetadata ? metadataChange : null;
        }

        // Convert a Metadata object to a string.
        protected string MetadataToString(StorageMetadata metadata, bool onlyMutableFields)
        {
            var fieldsAndValues = new Dictionary<string, object> {
        {"ContentType", metadata.ContentType},
        {"CacheControl", metadata.CacheControl},
        {"ContentDisposition", metadata.ContentDisposition},
        {"ContentEncoding", metadata.ContentEncoding},
        {"ContentLanguage", metadata.ContentLanguage}
      };
            if (!onlyMutableFields)
            {
                foreach (var kv in new Dictionary<string, object> {
                            {"Reference", metadata.Reference != null ?
                                              metadata.Reference.Path : null},
                            {"Path", metadata.Path},
                            {"Name", metadata.Name},
                            {"Bucket", metadata.Bucket},
                            {"Generation", metadata.Generation},
                            {"MetadataGeneration", metadata.MetadataGeneration},
                            {"CreationTimeMillis", metadata.CreationTimeMillis},
                            {"UpdatedTimeMillis", metadata.UpdatedTimeMillis},
                            {"SizeBytes", metadata.SizeBytes},
                            {"Md5Hash", metadata.Md5Hash}
                         })
                {
                    fieldsAndValues[kv.Key] = kv.Value;
                }
            }
            foreach (var key in metadata.CustomMetadataKeys)
            {
                fieldsAndValues[key] = metadata.GetCustomMetadata(key);
            }
            var fieldAndValueStrings = new List<string>();
            foreach (var kv in fieldsAndValues)
            {
                fieldAndValueStrings.Add(String.Format("{0}={1}", kv.Key, kv.Value));
            }
            return String.Join("\n", fieldAndValueStrings.ToArray());
        }
    }
}