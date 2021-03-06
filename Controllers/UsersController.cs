﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        //private readonly DataContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, IMapper mapper,
            IPhotoService photoService, ILogger<UsersController> logger)
        {
            //_context = context;
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            userParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "male" ? "female" : "male";

            PagedList<MemberDto> users = await _userRepository.GetUsersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            //_logger.LogWarning("Testing Logger");

            return Ok(users);
        }


        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            return Ok(_mapper.Map<MemberDto>(user));
        }

        [HttpGet("secondWay/{username}")]
        public async Task<ActionResult<MemberDto>> GetUserSecondWay(string username)
        {
            return Ok(await _userRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
                return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
                photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
                //return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser", new { user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain)
                return BadRequest("This is already your main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to set main photo");
        }


        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
               return NotFound();

            if (photo.IsMain)
               return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }


    }
}
