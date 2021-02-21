using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.Username == username);
        }

        // second way
        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users.Where(x => x.Username == username)
                    .Select(user => new MemberDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                        Age = user.GetAge(),
                        KnownAs = user.KnownAs,
                        Created = user.Created,
                        LastActive = user.LastActive,
                        Gender = user.Gender,
                        Introduction = user.Introduction,
                        LookingFor = user.LookingFor,
                        Interests = user.Interests,
                        City = user.City,
                        Country = user.Country,
                        Photos = user.Photos.Select(photo => new PhotoDto
                                            {
                                                Id = photo.Id,
                                                Url = photo.Url,
                                                IsMain = photo.IsMain
                                            }).ToList()
                    }).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto>> GetUsersAsync()
        {
            var users = await _context.Users.Include(p => p.Photos).ToListAsync();
            return _mapper.Map<IEnumerable<MemberDto>>(users);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
