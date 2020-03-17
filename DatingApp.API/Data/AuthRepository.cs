using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if(user == null)
                return null;

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        //Function VerifyPasswordHash có ba tham số password, passwordHash, passwordSalt. Trong đó nó sẽ
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {  
            //Khởi tạo hmac là một instance của HMACSHA512 dựa vào key passwordSalt. Là một key bí mật dùng để truy cập đến passwordHash và dựa vào khóa này để mã hóa Password thành PasswordHash
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                //computedHash = hmac dùng khóa cùng khóa với passwordSalt để mã hóa password thành passwordHash
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i = 0; i < computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i])
                        return false;
                }
                return true;
            } 
        }

        public async Task<User> Register(User user, string password)
        {
            //Chú ý rằng user chỉ lưu giữ bốn thuộc tính là Id, UserName, PasswordHash và PasswordSalt
            byte[] passwordHash, passwordSalt;  //Khai báo hai biến có kiểu là một mảng byte
            CreatePasswordHash(password, out passwordHash, out passwordSalt);   //Truyền hai biến trên vào hàm CreatePasswordHash. từ khóa out sẽ nhận giá trị vào sau đó reset giá trị và lưu giữ giá trị khi ra khỏi hàm.

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);    //Thêm bất đồng bộ value user vào entity Users
            await _context.SaveChangesAsync();  //Lưu

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            //Khởi tạo hmac là một thể hiện mới của lớp HMACSHA512 với một key được tạo random
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;    //Một Key random dùng để mở khóa để giải mã passwordHash về lại thành password
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  //PasswordHash = ComputeHash(password)
                //passwordHash là giá trị sau khi được mã hóa từ password.
            }
        }


        //UserExists có một tham số username thực hiện việc kiểm tra xem có bất cứ row nào trong entity Users hay không.
        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }
    }
}