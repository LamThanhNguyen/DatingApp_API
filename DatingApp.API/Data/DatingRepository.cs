using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            //user = _context trong table Users relations với tables Photos có Id == id nhập vào.

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            //users = _context trong table User relations với các Photos có id liên kết.

            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
                // userLikers == các id đã like cho UserId
                //users = users mà có id là id của các id trên.
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
                // userLikees == các id được UserId like. 
                //Chúng ta vẫn để đối số thứ hai của GetUserLikes là userParams.Likers bởi vì
                //hiện tại giá trị là userParams.Likees chứ không phải userParams.Likers 
                //Vì vậy khối điều kiện If --- else sẽ chạy khối lệnh else.
                //users = users mà có id là id của các id trên.
            }

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize); 
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                        .Include(x => x.Likers)
                        .Include(x => x.Likees)
                        .FirstOrDefaultAsync(u => u.Id == id);

            //user = Trong Table Users ==> bản ghi == id nhập vào và các ids mà đã từng like hay được like từ id hiện tại

            if(likers)  //Nếu muốn truy xuất các người đã like cho id hiên tại
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
                // Trả về trong table user có các Likers mà đã từng Like id hiện tại sau đó trả về các ids của những người Like này
            }
            else    //Nếu muốn truy xuất các người đã được id hiện tại like // Hay nói là truy xuất những người không phải là những người đã like cho id hiện tại
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
                // Trả về trong table user có các Likees mà đã từng được id hiện tại Like sau đó trả về các ids của những người đã nhận được like này.
            }
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                    .Include(u => u.Sender).ThenInclude(p => p.Photos)
                    .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                    .AsQueryable();
            //messages = Trong Table Messages trả về Các Người Gửi (Các Hình ảnh liên kết)
            //              cúng với các Người Nhận (Các Hình ảnh liên kết)

            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId
                        && u.RecipientDeleted == false);
                    break;
                    //Nếu MessageContainer là Inbox thì lấy ra các message mà trong đó có Id người nhận bằng Id truyền vào và có Trang thái xóa thư của người nhận là false.
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId
                        && u.SenderDeleted == false);
                    break;
                    //Nếu MessageContainer là Outbox thì lấy ra các message mà trong đó có Id người gửi bằng Id truyền vào và có Trạng thái xóa thư của người gửi là false.
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId
                        && u.RecipientDeleted == false && u.IsRead == false);
                    break;
                    //Default: Nếu default thì lấy ra các message mà trong đó Id người nhận bằng Id truyền vào cùng với chưa xóa thư trong vai trong người Nhận và chưa đọc.
            }
            
            messages = messages.OrderByDescending(d => d.MessageSent);
            //Xắp xếp giảm dần thời gian các tin nhắn gửi đi.

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
            //Call Method CreateAsync với các tham số vừa tạo ra trong đó chủ yếu là thêm các messages vào trong List<Message> 
            //Lưu ý là List<Message> chứ không phải table Message
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientId == userId && m.SenderId == recipientId && m.RecipientDeleted == false
                    || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
            
            return messages;

            //A là userId và B là recipientId
            //Tập Mệnh đề đầu tiên:
            //  A là người nhận và B là người gửi và tin nhắn của A chưa xóa.
            //Tập Mệnh đề thứ hai:
            //  A là người gửi và B là người nhận và tin nhắn của A chưa xóa.
        }
    }
}