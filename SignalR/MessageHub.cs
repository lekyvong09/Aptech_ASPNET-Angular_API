using System;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();

            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;

            var groupName = GetGroupName(username, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepository.GetMessageThread(username, otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }


        public async Task SendMessage (CreateMessageDto createMessageDto)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;

            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("you cannot send messages to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null)
                throw new HubException("not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
            {
                var group = GetGroupName(username, recipient.UserName);
                await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

        }


        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
