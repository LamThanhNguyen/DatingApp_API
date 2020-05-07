namespace DatingApp.API.Models
{
    public class Like
    {
        public int LikerId { get; set; }    //Id người Like
        public int LikeeId { get; set; }    //Id người nhận Like
        public User Liker { get; set; }     //Người Like
        public User Likee { get; set; }     //Người nhận Like
    }
}