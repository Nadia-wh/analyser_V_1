using System.ComponentModel.DataAnnotations;

namespace demo_analyser.Models
{
    public class AnswerSheets
    {
        [Key]
        public int AP_Id { get; set; } // primary key for each answersheet

        [Required]
        public string AP_Link { get; set; } //the saved url of file in each blob

        [Required]
        public string Upload_Status { get; set; } // the status of file after uploading 
    }
}
