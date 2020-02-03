using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Ads
{
    public class AdCommentRequest
    {
        [Required]
        public string Text { get; set; }
    }
}
