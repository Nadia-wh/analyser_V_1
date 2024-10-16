using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace demo_analyser.Controllers
{
    public class UploadController : Controller
    {
      
        // Connection string of blob 
        private string _connectionString = "DefaultEndpointsProtocol=https;AccountName=eualvatordata;AccountKey=10gdRTBTKGyypVKdXUqVF4jy4mBBIPnCBZDksb25fMQ2EK2TcAKmTvzy9+LWUX6XWILywMOZhI/u+ASt7igF+Q==;EndpointSuffix=core.windows.net";

        //Connection string of database
        private readonly string _dbConnectionString;

        public UploadController(IConfiguration configuration)
        {
            _dbConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet("ListUploadedFiles")]
        public async Task<IActionResult> ListUploadedFiles()
        {
            try
            {
                BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, "democontainer");

                // Check if the container exists
                if (!await blobContainerClient.ExistsAsync())
                {
                    return NotFound("Container does not exist.");
                }

                // List blobs in the container
                List<string> blobUrls = new List<string>();
                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    // Get the blob client for the blob item
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                    // Add the blob's URL to the list
                    blobUrls.Add(blobClient.Uri.ToString());
                }

                // Return the list of blob URLs
                return View(blobUrls); // Ensure a view is created to display this list
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("UploadFilesToStorage")]
        public async Task<IActionResult> UploadFilesToStorage([FromForm] IList<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files were uploaded.");
            }

            try
            {
                // Initialize the BlobContainerClient
                BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, "democontainer");

                // Create the container if it does not exist
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                foreach (IFormFile file in files)
                {

                    //including folder
                    //BlobClient blobClient = blobContainerClient.GetBlobClient($"folder1/folder2/{file.FileName}");
                    BlobClient blobClient = blobContainerClient.GetBlobClient($"grade/question/{file.FileName}");


                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;

                        // Upload the blob with the overwrite option set to true
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    // Save file metadata to the database using the new model structure
                    var uploadStatus = "Success"; // Set upload status
                    var fileUrl = blobClient.Uri.ToString(); // Get the full URL of the uploaded blob
                    SaveFileMetadataToDatabase(fileUrl, uploadStatus);

                }


                return Ok("Files uploaded successfully.");



            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 status code with the error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        private void SaveFileMetadataToDatabase(string fileUrl, string uploadStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_dbConnectionString))
                {
                    connection.Open();

                    // Check if the file URL already exists
                    string checkQuery = "SELECT COUNT(1) FROM AnswerSheets WHERE AP_Link = @AP_Link";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@AP_Link", fileUrl);
                        int exists = (int)checkCommand.ExecuteScalar();

                        if (exists > 0)
                        {
                            Console.WriteLine("File URL already exists in the database.");
                            return; // Skip the insertion
                        }
                    }

                    // If not existing, insert the new record
                    string insertQuery = "INSERT INTO AnswerSheets (AP_Link, Upload_Status) VALUES (@AP_Link, @Upload_Status)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@AP_Link", fileUrl);
                        insertCommand.Parameters.AddWithValue("@Upload_Status", uploadStatus);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
            }
        }





    }
}
//https://eualvatordata.blob.core.windows.net/democontainer/grade/question/answer.pdf
//https://eualvatordata.blob.core.windows.net/democontainer/grade/question/answer_3.pdf
//https://eualvatordata.blob.core.windows.net/democontainer/grade/question/answer.pdf