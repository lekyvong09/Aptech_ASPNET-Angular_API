using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
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
            var user = await _context.Users.Where(x => x.Username == username)
                    .SingleOrDefaultAsync();
            return _mapper.Map<MemberDto>(user);
        }

        public async Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams)
        {
            var query = _context.Users
                        .Include(p => p.Photos)
                        .Where(u => u.Username != userParams.CurrentUsername && u.Gender == userParams.Gender)
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
                        }).OrderBy(x => x.Id).AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
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
