using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> 
    {
        //constructor con kế thừa contructor của cha.(IdentityDbContext)
        public DataContext(DbContextOptions<DataContext> options): base(options) {}      
        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos {get; set;}
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            //Gọi Method của cha
            base.OnModelCreating(builder);

            builder.Entity<UserRole>(userRole => 
            {
                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<Like>()
                .HasKey(k => new {k.LikerId, k.LikeeId});

            builder.Entity<Like>()
                .HasOne(u => u.Likee)       //Một người có thể được nhiều người Like
                .WithMany(u => u.Likers)    //In Table Users
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            
            //Many To Many (Likee // Liker)

            builder.Entity<Like>()
                .HasOne(u => u.Liker)       //Một người có thể Like nhiều người
                .WithMany(u => u.Likees)    //In Table Users
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)  //Một người có thể nhắn nhiều tin nhắn
                .WithMany(u => u.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)   //Một người nhận có thể nhận được nhiều tin nhắn
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

///Một User có nhiều Photos. Nhiều Photos thuộc về một User.
///Một User được nhiều người khác Like
//Một User có thể Like nhiều người khác


//Entity Message Một Người Gửi Có thể gửi nhiều tin nhắn cho một hoặc nhiều người.
//Entity Message Một Người Nhận có thể nhận nhiều tin nhắn từ một hoặc nhiều người.
//Khi Xóa Một entity trong table chính thì table phụ sẽ nhận giá trị null