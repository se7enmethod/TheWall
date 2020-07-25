using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheWall.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Post a comment")]
        public string CommentContent { get; set; }

        // One-To-Many (One-side) nav property goes here <<

        public int UserId { get; set; }
        public User User { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; }
        // One-To-Many (Many-side) nav property goes here <<


        // Many-To-Many nav property goes here <<

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

